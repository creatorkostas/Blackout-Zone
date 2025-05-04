using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;

namespace Nicrom.PM {
    public class TextureGrid_Window : EditorWindow{

        [SerializeField]
        private TextureGrid tg;
        /// <summary>
        /// Used for side panel scrolling.
        /// </summary>
        private Vector2 scroll;
        /// <summary>
        /// Previous window size.
        /// </summary>
        private Vector2 prevWinSize;
        /// <summary>
        /// Used to determine if the canvas should be centered when the Grid Editor is opened.
        /// </summary>
        private static bool centerCanvasOnEnable = false;
        private Vector2 mousePos = Vector2.zero;
        private const float SCROLL_MODIFIER = 1.2f;
        private const float MAX_CANVAS_SCALE_SCROLL = 20f;
        private const float MIN_CANVAS_SCALE = 0.0001f;
        private const float MAX_CANVAS_SCALE = 250f;
        private float canvasScale = 1f;

#if UNITY_EDITOR

        public static void ShowWindow(TextureGrid tg)
        {
            TextureGrid_Window window = GetWindow<TextureGrid_Window>();
            window.tg = tg;
            centerCanvasOnEnable = true;

            if (tg.texAtlasSize == Vector2Int.zero && tg.texAtlas != null)
                tg.texAtlasSize = new Vector2Int(tg.texAtlas.width, tg.texAtlas.height);

            tg.SetRef();
        }

        private void OnEnable()
        {
            wantsMouseMove = true;
            titleContent = new GUIContent("Grid Editor");
            minSize = new Vector2(925, 625);
            prevWinSize = minSize;

            if (tg != null)
            {
                tg.SetRef();

                tg.originOffset =  Vector2.zero;
                tg.sizeOffset = Vector2Int.zero;
            }
            centerCanvasOnEnable = true;
        }

        void OnGUI()
        {
            if (tg.texAtlas == null)
                return;

            if(centerCanvasOnEnable)
            {
                FitCanvasOnScreen();
                centerCanvasOnEnable = false;
                prevWinSize = position.size;
            }

            if(tg.drawGrid)
                DrawBackgroundGrids();
            if(tg.drawAxes)
                DrawAxes();

            EditorGUILayout.BeginVertical();
            DrawTextureAtlas();
            DrawCustomGrids();
            EditorGUILayout.EndVertical();

            if (tg.showPanel)
                DrawSidePanel();

            ProcessGridEvents(Event.current);
            ProcessEvents(Event.current);

            if (GUI.changed)
                Repaint();
        }

        private void OnSelectionChange()
        {
            foreach (UnityEngine.Object o in Selection.objects)
            {
                if (o.GetType() == typeof(TextureGrid))
                {
                    tg = o as TextureGrid;
                    centerCanvasOnEnable = true;
                    Repaint();
                    break;
                }
            }
        }

        /// <summary>
        /// Draws the background grids.
        /// </summary>
        private void DrawBackgroundGrids()
        {
            float scale = 1f;

            if (tg.sizeOffset.x != 0)
                scale = (float)(tg.sizeOffset.x + tg.texAtlas.width) / (float)tg.texAtlas.width;

            DrawBackgroundGrid((tg.texAtlas.width / 16) * scale, 0.2f, new Color(0.5f, 0.5f, 0.5f, 1.0f));
            DrawBackgroundGrid((tg.texAtlas.width / 4) * scale, 0.4f, new Color(0.8f, 0.8f, 0.8f, 1.0f));
        }

        /// <summary>
        /// Draws a background grid.
        /// </summary>
        /// <param name="gridSpacing"> The space between the grid lines. </param>
        /// <param name="gridOpacity"> The opacity of the grid lines. </param>
        /// <param name="gridColor"> The color of the grid lines. </param>
        private void DrawBackgroundGrid(float gridSpacing, float gridOpacity, Color gridColor)
        {
            int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
            int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

            Handles.BeginGUI();
            Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

            Vector3 newOffset = new Vector3(tg.originOffset.x % gridSpacing, tg.originOffset.y % gridSpacing, 0);

            for (int i = 0; i < widthDivs + 2; i++)
            {
                Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing * i + newOffset.x, position.height + 100, 0f));
            }

            for (int j = 0; j < heightDivs + 2; j++)
            {
                Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3(position.width, gridSpacing * j + newOffset.y, 0f));
            }

            Handles.color = Color.white;
            Handles.EndGUI();
        }

        /// <summary>
        /// Draws the X and Y axes.
        /// </summary>
        private void DrawAxes()
        {
            Handles.BeginGUI();

            Handles.color = Color.red;
            Handles.DrawLine(new Vector3(0f, tg.originOffset.y + tg.texAtlas.height + tg.sizeOffset.y, 0f), new Vector3(position.width, tg.originOffset.y + tg.texAtlas.height + tg.sizeOffset.y, 0f));

            Handles.color = Color.green;
            Handles.DrawLine(new Vector3(tg.originOffset.x, 0, 0f), new Vector3(tg.originOffset.x, position.height, 0f));

            Handles.color = Color.white;
            Handles.EndGUI();
        }

        /// <summary>
        /// Draws the texture atlas.
        /// </summary>
        private void DrawTextureAtlas()
        {
            if (tg.texAtlas == null)
                return;

            EditorGUI.DrawPreviewTexture(new Rect(tg.originOffset.x, tg.originOffset.y, tg.texAtlas.width + tg.sizeOffset.x, tg.texAtlas.height + tg.sizeOffset.y), tg.texAtlas);
        }

        /// <summary>
        /// Draws the custom grids.
        /// </summary>
        private void DrawCustomGrids()
        {
            for (int i = 0; i < tg.gridsList.Count; i++)
            {
                tg.gridsList[i].DrawCustomGrid();
            }
        }

        /// <summary>
        /// Draws the side panel.
        /// </summary>
        private void DrawSidePanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(tg.sidePanelWidth));
            GUI.Box(new Rect(0, 0, tg.sidePanelWidth, position.height), GUIContent.none);

            EditorGUILayout.BeginVertical();
            DrawViewportOptions();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            DrawCustomGridProperties();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Draws the viewport options in the side panel.
        /// </summary>
        private void DrawViewportOptions()
        {
            GUILayout.BeginVertical("RL Background", GUILayout.Height(30));
            GUILayout.Space(5);

            EditorGUILayout.LabelField(new GUIContent("View"), EditorStyles.boldLabel);

            GUILayout.Space(5);
            if (GUILayout.Button(new GUIContent("Fit on Screen")))
            {
                FitCanvasOnScreen();
            }

            EditorGUI.BeginChangeCheck();
            tg.drawAxes = EditorGUILayout.Toggle(new GUIContent("Axes", "Enables/Disables the viewport axes."), tg.drawAxes);
            tg.drawGrid = EditorGUILayout.Toggle(new GUIContent("Grid", "Enables/Disables the viewport background grid."), tg.drawGrid);
            if (EditorGUI.EndChangeCheck())
            {
                GUI.changed = true;
            }

            EditorGUI.BeginChangeCheck();
            tg.canvasBorder = Mathf.Clamp(EditorGUILayout.IntField(new GUIContent("Canvas Border", "Size of the border that is added when the canvas is scaled and centered."), tg.canvasBorder), 0, 200);
            if (EditorGUI.EndChangeCheck())
            {
                FitCanvasOnScreen();
            }

            //tg.zoomSpeed = Mathf.Clamp(EditorGUILayout.IntField(new GUIContent("Zoom Speed", "Determines how fast Zoom In/Zoom Out is."), tg.zoomSpeed), 1, 500);
            tg.handleSize = Mathf.Clamp(EditorGUILayout.IntField(new GUIContent("Handle Size", "The size of a grid handle."), tg.handleSize), 1, 100);

            GUILayout.Space(10);
            EditorGUILayout.LabelField(new GUIContent("Grid Color"), EditorStyles.boldLabel);

            tg.gridColorMode = (GridColorMode)EditorGUILayout.EnumPopup(new GUIContent("Color Mode", ""), tg.gridColorMode);

            if (tg.gridColorMode == GridColorMode.SingleColor)
                tg.gridColor = EditorGUILayout.ColorField(new GUIContent("Color", "Color to apply to all the grids."), tg.gridColor);

            if (GUILayout.Button(new GUIContent("Apply Color")))
            {
                for (int i = 0; i < tg.gridsList.Count; i++)
                {
                    if (tg.gridColorMode == GridColorMode.SingleColor)
                        tg.gridsList[i].gridColor = tg.gridColor;
                    else
                        tg.gridsList[i].gridColor = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);

                    Repaint();
                }
            }

            GUILayout.Space(10);

            BoldFontStyle(() =>
            {
                EditorGUILayout.BeginHorizontal();
                tg.showShortcuts = EditorGUILayout.Foldout(tg.showShortcuts, "Help");
                EditorGUILayout.EndHorizontal();
            });

            if (tg.showShortcuts)
            {
                EditorGUILayout.HelpBox("Select grid - Left mouse button", MessageType.None);
                EditorGUILayout.HelpBox("Deselect grid - Click outside the grid", MessageType.None);
                EditorGUILayout.HelpBox("Viewport zoom - Use mouse scroll wheel or press +/- keys.", MessageType.None);
                EditorGUILayout.HelpBox("Vioewport drag - Hold middle mouse button and drag. ALT + left mouse button click and drag", MessageType.None);
                EditorGUILayout.HelpBox("Open contex menu - Right mouse button", MessageType.None);
                EditorGUILayout.HelpBox("Delete grid - Delete key", MessageType.None);
                EditorGUILayout.HelpBox("Select a locked grid - SHIFT + left mouse button", MessageType.None);
                EditorGUILayout.HelpBox("Add/Remove grid columns - Left/Right arrow keys", MessageType.None);
                EditorGUILayout.HelpBox("Add/Remove grid rows - Up/Down arrow keys", MessageType.None);
                EditorGUILayout.HelpBox("Focus sidel panel on selected grid - F key", MessageType.None);
            }

            GUILayout.Space(5);
            GUILayout.EndVertical();
            GUILayout.Space(10);
        }

        /// <summary>
        /// Draws the grid properties in the side panel.
        /// </summary>
        private void DrawCustomGridProperties()
        {
            int len = tg.gridsList.Count;
            scroll = EditorGUILayout.BeginScrollView(scroll);
            string label = "";

            for (int i = 0; i < tg.gridsList.Count; i++)
            {
                if (tg.gridsList[i].isSelected)
                    label = tg.gridsList[i].gridLabel + " - Selected";
                else
                    label = tg.gridsList[i].gridLabel;

                GUILayout.BeginVertical("RL Header");
                GUILayout.Space(1);
                GUILayout.BeginHorizontal();
                GUILayout.Space(6);

                BoldFontStyle(() =>
                {
                    EditorGUILayout.BeginHorizontal();
                    tg.gridsList[i].showGridOptions = EditorGUILayout.Foldout(tg.gridsList[i].showGridOptions, label);
                    EditorGUILayout.EndHorizontal();
                });

                GUILayout.EndHorizontal();
                GUILayout.EndVertical();

                if (tg.gridsList[i].showGridOptions)
                {
                    GUILayout.BeginVertical("RL Background", GUILayout.Height(30));
                    GUILayout.Space(5);

                    if (tg.gridsList[i].editGridName)
                        tg.gridsList[i].gridLabel = EditorGUILayout.TextField(new GUIContent("Grid Name"), tg.gridsList[i].gridLabel);

                    tg.gridsList[i].isSelectionLocked = EditorGUILayout.Toggle(new GUIContent("Lock Selection", "Locks the left mouse button selection for this item. "
                        + " The item now can be selected with ALT key + left mouse button."), tg.gridsList[i].isSelectionLocked);
                    tg.gridsList[i].isGridPosLocked = EditorGUILayout.Toggle(new GUIContent("Lock Position", "Locks the ability to move a grid."), tg.gridsList[i].isGridPosLocked);
                    tg.gridsList[i].gridColor = EditorGUILayout.ColorField(new GUIContent("Grid Color", "Grid color"), tg.gridsList[i].gridColor);

                    EditorGUI.BeginChangeCheck();
                    tg.gridsList[i].gridPos = EditorGUILayout.Vector2IntField(new GUIContent("Pos", "Grid position in texture space."), tg.gridsList[i].gridPos);
                    tg.gridsList[i].gridPos = TG_Utils.ClampRectPosToTexture(tg, tg.gridsList[i].gridPos, new Vector2(tg.gridsList[i].gridWidth, tg.gridsList[i].gridHeight));
                    if (EditorGUI.EndChangeCheck())            
                        UpdateGridOnPosChange(i);
                    
                    GUILayout.Space(4);

                    EditorGUI.BeginChangeCheck();
                    tg.gridsList[i].gridWidth = EditorGUILayout.IntField(new GUIContent("Width", "Grid width in pixels."), tg.gridsList[i].gridWidth);
                    tg.gridsList[i].gridWidth = TG_Utils.ClampRectWidth(tg, tg.gridsList[i]);
                    if (EditorGUI.EndChangeCheck())
                        UpdateGridOnWidthChange(i);

                    EditorGUI.BeginChangeCheck();
                    tg.gridsList[i].gridHeight = EditorGUILayout.IntField(new GUIContent("Height", "Grid height in pixels."), tg.gridsList[i].gridHeight);
                    tg.gridsList[i].gridHeight = TG_Utils.ClampRectHeight(tg, tg.gridsList[i]);
                    if (EditorGUI.EndChangeCheck())
                        UpdateGridOnHeightChange(i);

                    if (!tg.gridsList[i].isTexPattern)
                    {
                        GUILayout.Space(4);

                        EditorGUI.BeginChangeCheck();
                        tg.gridsList[i].gridColumns = EditorGUILayout.IntField(new GUIContent("Columns", "The number of columns the grid has."), tg.gridsList[i].gridColumns);
                        tg.gridsList[i].gridColumns = TG_Utils.ClampGridColumns(tg.gridsList[i]);
                        if (EditorGUI.EndChangeCheck())
                            UpdateGridOnColumnChange(i);

                        EditorGUI.BeginChangeCheck();
                        tg.gridsList[i].gridRows = EditorGUILayout.IntField(new GUIContent("Rows", "The number of rows the grid has."), tg.gridsList[i].gridRows);
                        tg.gridsList[i].gridRows = TG_Utils.ClampGridRows(tg.gridsList[i]);
                        if (EditorGUI.EndChangeCheck())
                            UpdateGridOnRowChange(i);
                    }

                    if(!tg.gridsList[i].isTexPattern)
                        tg.gridsList[i].hasEmptySpace = EditorGUILayout.Toggle(new GUIContent("Has Empty Space", "Used to determine if this grid has color cells that are unused."), tg.gridsList[i].hasEmptySpace);

                    if (tg.gridsList[i].hasEmptySpace)
                    {
                        tg.gridsList[i].emptySpaceColor = EditorGUILayout.ColorField(new GUIContent("Empty Space Color", "The color of the empty space on the texture."), tg.gridsList[i].emptySpaceColor);
                    }

                    GUILayout.Space(10);
                    EditorGUILayout.BeginHorizontal();

                    if (tg.gridsList.Count > 1)
                    {
                        if (GUILayout.Button(new GUIContent("-", "Deletes the current grid."), EditorStyles.miniButton, GUILayout.Width(20)))
                        {
                            tg.gridsList.RemoveAt(i);
                        }
                    }

                    if (GUILayout.Button(new GUIContent("E", "Edit Grid name."), EditorStyles.miniButton, GUILayout.Width(20)))
                    {
                        tg.gridsList[i].editGridName = !tg.gridsList[i].editGridName;
                    }

                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(5);
                    GUILayout.EndVertical();
                }
                GUILayout.Space(10);
            }
            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// Updates the canvas data of a grid when its position is modified from the side panel.
        /// </summary>
        /// <param name="gridIndex"> Index of a CustomGrid item. </param>
        private void UpdateGridOnPosChange(int gridIndex)
        {
            tg.gridsList[gridIndex].FromTextureTexelToCanvasPixel();
        }

        /// <summary>
        /// Updates the canvas data of a grid when its height is modified from the side panel.
        /// </summary>
        /// <param name="gridIndex"> Index of a CustomGrid item. </param>
        private void UpdateGridOnHeightChange(int gridIndex)
        {
            tg.gridsList[gridIndex].FromTextureTexelToCanvasPixel();
            tg.gridsList[gridIndex].UpdateHorizontalLinesPos();
        }

        /// <summary>
        /// Updates the canvas data of a grid when its width is modified from the side panel.
        /// </summary>
        /// <param name="gridIndex"> Index of a CustomGrid item. </param>
        private void UpdateGridOnWidthChange(int gridIndex)
        {
            tg.gridsList[gridIndex].FromTextureTexelToCanvasPixel();
            tg.gridsList[gridIndex].UpdateVerticalLinesPos();
        }

        /// <summary>
        /// Updates the grid data when the columns number is modified from the side panel.
        /// </summary>
        /// <param name="gridIndex"> Index of a CustomGrid item. </param>
        private void UpdateGridOnColumnChange(int gridIndex)
        {
            tg.gridsList[gridIndex].RebuildVerticalLines();
        }

        /// <summary>
        /// Updates the grid data when the rows number is modified from the side panel.
        /// </summary>
        /// <param name="gridIndex"> Index of a CustomGrid item. </param>
        private void UpdateGridOnRowChange(int gridIndex)
        {
            tg.gridsList[gridIndex].RebuildHorizontalLines();
        }

        /// <summary>
        /// Processes mouse and keyboard events.
        /// </summary>
        public void ProcessEvents(Event e)
        {
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == 0)
                    {
                        if(e.mousePosition.x > tg.sidePanelWidth)
                            GUI.FocusControl(null);

                        mousePos = e.mousePosition;
                    }

                    if (e.button == 1 && e.mousePosition.x > tg.sidePanelWidth)
                    {
                        ProcessContextMenu(e.mousePosition);
                    }
                    break;

                case EventType.MouseDrag:
                    if (e.button == 2 && e.mousePosition.x > tg.sidePanelWidth)
                    {
                        OnCanvasDrag(e.delta);
                    }

                    if (e.alt && e.button == 0)
                    {
                        OnCanvasDrag(e.mousePosition - mousePos);
                        mousePos = e.mousePosition;
                    }
                    break;

                case EventType.ScrollWheel:
                    if (e.mousePosition.x > tg.sidePanelWidth)
                    {
                        OnCanvasZoom(e.delta);
                    }
                    break;
                case EventType.KeyDown:
                    if (e.keyCode == KeyCode.Delete)
                    {
                        DeleteGrid();
                    }
                    if (e.keyCode == KeyCode.F && tg.showPanel && e.mousePosition.x < tg.sidePanelWidth)
                    {
                        FocusOnSelectedGrid();
                    }

                    if(e.keyCode == KeyCode.KeypadPlus || e.keyCode == KeyCode.Equals)
                    {
                        GUI.FocusControl(null);
                        OnCanvasZoom(new Vector2(0, -3));
                    }

                    if (e.keyCode == KeyCode.KeypadMinus || e.keyCode == KeyCode.Minus)
                    {
                        GUI.FocusControl(null);
                        OnCanvasZoom(new Vector2(0, 3));
                    }
                    break;
            }

            CenterCanvasOnWindowResize(position.size);
            CheckGridsOverlap();
        }

        private void CheckGridsOverlap()
        {
            int count = tg.gridsList.Count;

            for (int i = 0; i < count; i++)
            {
                tg.gridsList[i].isOverlapping = false;
            }

            int start = 0;

            for (int i = 0; i < count - 1; i++)
            {
                for (int j = start; j < count - 1; j++)
                {
                    if(TG_Utils.AreGridsOverlapping(tg.gridsList[i], tg.gridsList[j + 1]))
                    {
                        tg.gridsList[i].isOverlapping = true;
                        tg.gridsList[j + 1].isOverlapping = true;
                    }
                }

                start++;
            }
        }

        /// <summary>
        /// Focuses the side panel on the selected item.
        /// </summary>
        private void FocusOnSelectedGrid()
        {
            for (int i = 0; i < tg.gridsList.Count; i++)
            {
                if (tg.gridsList[i].isSelected)
                    tg.gridsList[i].showGridOptions = true;
                else
                    tg.gridsList[i].showGridOptions = false;

                GUI.changed = true;
            }
        }

        /// <summary>
        /// Calculates the origin offset of the canvas.
        /// </summary>
        /// <param name="delta"> Offset. </param>
        private void OnCanvasDrag(Vector2 delta)
        {
            float offset = 0;
            Vector2 winSize = position.size;

            tg.originOffset += delta;
            offset = (tg.sidePanelWidth + 32) - (tg.texAtlas.width + tg.sizeOffset.x);

            if (tg.originOffset.x < offset)
                tg.originOffset.x = offset;

            if (tg.originOffset.x > winSize.x - 32)
                tg.originOffset.x = winSize.x - 32;

            offset = -(tg.texAtlas.height + tg.sizeOffset.y) + 32;
            if (tg.originOffset.y < offset)
                tg.originOffset.y = offset;

            if (tg.originOffset.y > winSize.y - 32)
                tg.originOffset.y = winSize.y - 32;

            GUI.changed = true;
        }

        /// <summary>
        /// Calculates the size offset of the canvas.
        /// </summary>
        /// <param name="delta"> Offset. </param>
        private void OnCanvasZoom(Vector2 delta)
        {
            Vector2 sOffset = new Vector2Int(0, 0);
            float zoom = canvasScale - delta.y * ((canvasScale / MAX_CANVAS_SCALE_SCROLL) * SCROLL_MODIFIER);
            float newScale = Mathf.Clamp(zoom, MIN_CANVAS_SCALE, MAX_CANVAS_SCALE);

            sOffset.x = tg.texAtlas.width * newScale - tg.texAtlas.width;
            sOffset.y = (sOffset.x * tg.texAtlas.height) / tg.texAtlas.width;

            if ((tg.texAtlas.width + sOffset.x) > 64 && (tg.texAtlas.height + sOffset.y) > 64)
            {
                canvasScale = newScale;
                tg.originOffset = SetOriginOffsetOnCanvasZoom(tg.sizeOffset.x - sOffset.x);

                tg.sizeOffset.x = Mathf.RoundToInt(sOffset.x);
                tg.sizeOffset.y = Mathf.RoundToInt(sOffset.y);

                UpdateGridsOnCanvasZoom();

                GUI.changed = true;
            }
        }

        /// <summary>
        /// Calculates the origin offset of the canvas.
        /// </summary>
        /// <param name="offset"> Amount by which to change the origin offset value. </param>
        public Vector2 SetOriginOffsetOnCanvasZoom(float offset)
        {
            Vector2 origOffset = Vector2.zero;
            Vector2 mousePosition = Event.current.mousePosition;

            mousePosition = ClampMousePosToCanvas(mousePosition);

            float xScale = (mousePosition.x - tg.originOffset.x) / (tg.texAtlasSize.x + tg.sizeOffset.x);
            float yScale = (mousePosition.y - tg.originOffset.y) / (tg.texAtlasSize.y + tg.sizeOffset.y);

            origOffset = tg.originOffset + new Vector2(offset * xScale, ((offset * tg.texAtlasSize.y) / tg.texAtlasSize.x) * yScale);

            origOffset.x = Mathf.RoundToInt(origOffset.x);
            origOffset.y = Mathf.RoundToInt(origOffset.y);

            return origOffset;
        }

        /// <summary>
        /// Clamps the mouse position to the canvas space.
        /// </summary>
        /// <param name="pos"> Position of a grid. </param>
        public Vector2 ClampMousePosToCanvas(Vector2 pos)
        {
            if (pos.x < tg.originOffset.x)
                pos.x = tg.originOffset.x;

            if (pos.y < tg.originOffset.y)
                pos.y = tg.originOffset.y;

            if (pos.x > (tg.texAtlasSize.x + tg.sizeOffset.x) + tg.originOffset.x)
                pos.x = (tg.texAtlasSize.x + tg.sizeOffset.x) + tg.originOffset.x;

            if (pos.y > (tg.texAtlasSize.y + tg.sizeOffset.y) + tg.originOffset.y)
                pos.y = (tg.texAtlasSize.y + tg.sizeOffset.y) + tg.originOffset.y;

            return pos;
        }

        /// <summary>
        /// Updates the custom grids when the size of the canvas changed.
        /// </summary>
        private void UpdateGridsOnCanvasZoom()
        {
            for (int i = 0; i < tg.gridsList.Count; i++)
            {
                tg.gridsList[i].FromTextureTexelToCanvasPixel();
                tg.gridsList[i].FromTextureGridsToCanvasGrids();
            }

            GUI.changed = true;
        }

        /// <summary>
        /// Processes the context menu actions.
        /// </summary>
        /// <param name="mousePosition"> Current position of the mouse on the screen. </param>
        private void ProcessContextMenu(Vector2 mousePosition)
        {
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent("New Flat Color Grid"), false, () => OnClickAddFlatColorGrid(mousePosition));
            genericMenu.AddItem(new GUIContent("New Texture Rect"), false, () => OnClickAddTextureRect(mousePosition));

            genericMenu.AddSeparator("");
            genericMenu.AddItem(new GUIContent("Fit on Screen"), false, () => FitCanvasOnScreen());

            if (tg.showPanel)
                genericMenu.AddItem(new GUIContent("Hide Side Panel"), false, () => HideSidePanel());
            else
                genericMenu.AddItem(new GUIContent("Show Side Panel"), false, () => ShowSidePanel());

            genericMenu.AddSeparator("");
            if (tg.copyList.Count == 1)             
                genericMenu.AddItem(new GUIContent("Paste"), false, () => OnClickPasteCustomGrid(mousePosition));  
            else       
                genericMenu.AddDisabledItem(new GUIContent("Paste"));         

            genericMenu.ShowAsContext();
        }

        /// <summary>
        /// Shows the side panel.
        /// </summary>
        private void ShowSidePanel()
        {
            tg.showPanel = true;
            tg.sidePanelWidth = tg.panelDefaultWidth;
        }

        /// <summary>
        /// Hides the side panel.
        /// </summary>
        private void HideSidePanel()
        {
            tg.showPanel = false;
            tg.sidePanelWidth = 0;
        }

        /// <summary>
        /// Adds a new flat color grid.
        /// </summary>
        /// <param name="mousePosition"> Current position of the mouse on the screen. </param>
        private void OnClickAddFlatColorGrid(Vector2 mousePosition)
        {
            tg.ClearSelection();
            tg.gridsList.Add(new CustomGrid(tg, false, mousePosition));
        }

        /// <summary>
        /// Pastes a Flat Color Grid or Texture Rect on the canvas.
        /// </summary>
        /// <param name="mousePosition"> Current position of the mouse on the screen. </param>
        private void OnClickPasteCustomGrid(Vector2 mousePosition)
        {
            tg.ClearSelection();
            tg.gridsList.Add(new CustomGrid(tg, tg.copyList[0], mousePosition, true));
            GUI.changed = true;
        }

        /// <summary>
        /// Adds a new texture rect.
        /// </summary>
        /// <param name="mousePosition"> Current position of the mouse on the screen. </param>
        private void OnClickAddTextureRect(Vector2 mousePosition)
        {
            tg.ClearSelection();
            tg.gridsList.Add(new CustomGrid(tg, true, mousePosition));
        }

        private void ProcessGridEvents(Event e)
        {
            for (int i = 0; i < tg.gridsList.Count; i++)
            {
                tg.gridsList[i].ProcessEvents(e);      
            }
        }

        /// <summary>
        /// Centers the canvas when the window is resized.
        /// </summary>
        /// <param name="winSize"> The current size of the window. </param>
        private void CenterCanvasOnWindowResize(Vector2 winSize)
        {
            if(winSize.x != prevWinSize.x || winSize.y != prevWinSize.y)
            {
                tg.originOffset.x = (int)((winSize.x - tg.sidePanelWidth) * 0.5f - (tg.texAtlas.width + tg.sizeOffset.x) * 0.5f + tg.sidePanelWidth);
                tg.originOffset.y = (int)((winSize.y - (tg.texAtlas.height + tg.sizeOffset.y)) * 0.5f);
                prevWinSize = winSize;
                GUI.changed = true;
            }
        }

        /// <summary>
        /// Centers the canvas to the current window size.
        /// </summary>
        private void FitCanvasOnScreen()
        {
            Vector2 winSize = position.size;

            if (winSize.x - tg.sidePanelWidth > winSize.y)
            {
                if (tg.texAtlas.width > tg.texAtlas.height)
                {
                    if (tg.texAtlas.width / tg.texAtlas.height > (winSize.x - tg.sidePanelWidth) / winSize.y)
                        FitCanvasToXAxis(winSize);
                    else
                        FitCanvasToYAxis(winSize);
                }
                else
                {
                    FitCanvasToYAxis(winSize);
                }
            }
            else
            {
                if(tg.texAtlas.width > tg.texAtlas.height)
                {
                    FitCanvasToXAxis(winSize);
                }
                else
                {
                    if (tg.texAtlas.height / tg.texAtlas.width > winSize.y / (winSize.x - tg.sidePanelWidth))
                        FitCanvasToYAxis(winSize);
                    else
                        FitCanvasToXAxis(winSize);
                }
            }

            UpdateGridsOnCanvasZoom();
            GUI.changed = true;
        }

        /// <summary>
        /// Resizes the size of the canvas and its position, relative to the Y axis.
        /// </summary>
        /// <param name="winSize"> The current size of the window. </param>
        private void FitCanvasToYAxis(Vector2 winSize)
        {
            int x, y, newWidth, newHeight;

            tg.sizeOffset = new Vector2Int(0, 0);

            newHeight = (int)(winSize.y - tg.canvasBorder * 2);
            newWidth = newHeight * tg.texAtlas.width / tg.texAtlas.height;

            tg.sizeOffset.x = newWidth - tg.texAtlas.width;
            tg.sizeOffset.y = newHeight - tg.texAtlas.height;

            x = (int)((winSize.x - tg.sidePanelWidth) * 0.5f - newWidth * 0.5f + tg.sidePanelWidth);
            y = tg.canvasBorder;

            tg.originOffset.x = x;
            tg.originOffset.y = y;

            canvasScale = (tg.texAtlas.width + tg.sizeOffset.x) / (float)tg.texAtlas.width;
        }

        /// <summary>
        /// Resizes the size of the canvas and its position, relative to the X axis.
        /// </summary>
        /// <param name="winSize"> The current size of the window. </param>
        private void FitCanvasToXAxis(Vector2 winSize)
        {
            int x, y, newWidth, newHeight;

            newWidth = (int)(winSize.x - tg.sidePanelWidth - tg.canvasBorder * 2);
            newHeight = newWidth * tg.texAtlas.height / tg.texAtlas.width;

            tg.sizeOffset.x = newWidth - tg.texAtlas.width;
            tg.sizeOffset.y = newHeight - tg.texAtlas.height;

            x = (int)((winSize.x - tg.sidePanelWidth) * 0.5f - newWidth * 0.5f + tg.sidePanelWidth);
            y = (int)((winSize.y - newHeight) * 0.5f);

            tg.originOffset.x = x;
            tg.originOffset.y = y;

            canvasScale = (tg.texAtlas.width + tg.sizeOffset.x) / (float)tg.texAtlas.width;
        }

        /// <summary>
        /// Deletes a grid if one is selected.
        /// </summary>
        private void DeleteGrid()
        {
            for (int i = 0; i < tg.gridsList.Count; i++)
            {
                if (tg.gridsList[i].isSelected)
                {
                    tg.gridsList.RemoveAt(i);
                    Event.current.Use();
                    break;
                }
            }    
        }

        public void BoldFontStyle(Action inside)
        {
            GUIStyle style = EditorStyles.foldout;
            FontStyle previousStyle = style.fontStyle;
            style.fontStyle = FontStyle.Bold;
            inside();
            style.fontStyle = previousStyle;
        }
#endif
    }
}
