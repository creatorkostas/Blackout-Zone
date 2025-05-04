using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;

namespace Nicrom.PM {

    /// <summary>
    /// Determines where the handles are located.  
    /// </summary>
    public enum HandlesPos {
        /// <summary>
        /// The handles are located on the main rect of the grid. 
        /// Moving a handle, changes the width or height of the grid.  
        /// </summary>
        OnGridMainRect,
        /// <summary>
        /// The handles are located on the row lines.
        /// Moving a handle, changes the position of a row line inside the grid.  
        /// </summary>
        OnHorizontalLines,
        /// <summary>
        /// The handles are located on the column lines.
        /// Moving a handle, changes the position of a column line inside the grid.  
        /// </summary>
        OnVerticalLines
    }

    [Serializable]
    public class CustomGrid {
        #region Internal Fields
        [SerializeField]
        private HandlesPos handlesPos = HandlesPos.OnGridMainRect;
        /// <summary>
        /// The position on the X axis the grid vertical lines have. The position of the lines is in grid space. 
        /// The origin of the grid is at the top left corner. The grid pos/width/height is in canvas space. 
        /// </summary>
        [SerializeField]
        private List<float> vLinesOnCanvasGrid = new List<float>();
        /// <summary>
        /// The position on the Y axis the grid horizontal lines have. The position of the lines is in grid space. 
        /// The origin of the grid is at the top left corner. The grid pos/width/height is in canvas space. 
        /// </summary>
        [SerializeField]
        private List<float> hLinesOnCanvasGrid = new List<float>();
        /// <summary>
        /// Reference to a TextureGrid instance.
        /// </summary>
        [SerializeField]
        private TextureGrid tg;
        /// <summary>
        /// Position and size of the current grid in canvas space. The canvas has the origin at the top left corner.
        /// Y axis points downwards.
        /// </summary>
        [SerializeField]
        private Rect gridOnCanvas;
        /// <summary>
        /// The position inside a grid rect where left click was performed.
        /// </summary>
        public Vector2Int mouseInsideRectPos;
        /// <summary>
        /// The color a grid has in the Grid Editor window.
        /// </summary>
        public Color32 gridColor = Color.white;
        /// <summary>
        /// Name of the grid.
        /// </summary>
        public string gridLabel = "Flat Color Grid";
        /// <summary>
        /// Used to determine if a grid name should be drawn as a text field in the side panel.  
        /// </summary>
        public bool editGridName = false;
        /// <summary>
        /// Used to determine if a grid is selected in the Grid Editor window.  
        /// </summary>
        public bool isSelected = false;
        /// <summary>
        /// Used to determine if the grid properties should be drawn in the Grid Editor window.  
        /// </summary>
        public bool showGridOptions = true;
        /// <summary>
        /// Used to determine if left mouse selection is locked.  
        /// </summary>
        public bool isSelectionLocked = false;
        /// <summary>
        /// Used to determine if a grid handle is selected.  
        /// </summary>
        public bool isHandleSelected = false;
        /// <summary>
        /// Used to determine if the current grid is being dragged.
        /// </summary>
        private bool isDragged = false;
        /// <summary>
        /// Used to determine if the grid position is locked.  
        /// </summary>
        public bool isGridPosLocked = false;
        /// <summary>
        /// Used to determine if this grid is overlapping with another grid.
        /// </summary>
        public bool isOverlapping = false;
        /// <summary>
        /// The ID of a selected grid handle. 
        /// </summary>
        public int handleID = -1;
        #endregion Internal Fields

        /// <summary>
        /// The position on the X axis the grid vertical lines have. The position is in grid space. 
        /// The origin of the grid is at the bottom left corner. The grid pos/width/height is in texture pixel space. 
        /// </summary>
        public List<int> vLinesOnTexGrid = new List<int>();
        /// <summary>
        /// The position on the Y axis the grid horizontal lines have. The position is in grid space. 
        /// The origin of the grid is at the bottom left corner. The grid pos/width/height is in texture pixel space. 
        /// </summary>
        public List<int> hLinesOnTexGrid = new List<int>();
        /// <summary>
        /// The tint color that should be applied to a segment of the texture. 
        /// </summary>
        public Color32 tintColor = Color.white;
        /// <summary>
        /// The color of the texture parts that are not used by any models. 
        /// </summary>
        public Color32 emptySpaceColor = Color.white;
        /// <summary>
        /// The position of a grid. 
        /// </summary>
        public Vector2Int gridPos = new Vector2Int(0, 0);
        /// <summary>
        /// The width of the current grid. 
        /// </summary>
        public int gridWidth = 64;
        /// <summary>
        /// The height of the current grid. 
        /// </summary>
        public int gridHeight = 64;
        /// <summary>
        /// The number of columns the current grid has. 
        /// </summary>
        public int gridColumns = 3;
        /// <summary>
        /// The number of rows the current grid has. 
        /// </summary>
        public int gridRows = 3;
        /// <summary>
        /// Used to determine if the current grid has empty cells. 
        /// Empty cells represent parts of the texture that are unused.
        /// </summary>
        public bool hasEmptySpace = false;
        /// <summary>
        /// Used to determine if the region of the texture atlas 
        /// described by this grid is a texture pattern.
        /// </summary>
        public bool isTexPattern = false;

#if UNITY_EDITOR
        public CustomGrid(TextureGrid texGrid, bool isTex, Vector2 mousePosition)
        {
            tg = texGrid;
            isSelected = true;

            if (tg.texAtlas.width > 1024 && tg.texAtlas.height > 1024)
            {
                gridWidth = 128;
                gridHeight = 128;
            }

            OnNewGrid(isTex, mousePosition);
        }

        public CustomGrid(TextureGrid texGrid, CustomGrid cg, Vector2 mousePosition, bool selected)
        {
            tg = texGrid;
            isSelected = selected;
            gridPos = cg.gridPos;
            gridWidth = cg.gridWidth;
            gridHeight = cg.gridHeight;
            gridColumns = cg.gridColumns;
            gridRows = cg.gridRows;
            gridColor = cg.gridColor;

            OnNewGrid(cg.isTexPattern, mousePosition);
        }

        /// <summary>
        /// Sets different data when a new custom grid instance is created.
        /// </summary>
        /// <param name="isTex"> Used to determine if the region of the texture atlas
        /// described by this grid is a texture pattern. </param>
        /// <param name="mPos"> Current position of the mouse on the screen. </param>
        private void OnNewGrid(bool isTex, Vector2 mPos)
        {
            if (isTex)
                OnTexturePattern();

            FromTextureTexelToCanvasPixel();

            mPos = TG_Utils.ClampPosToCanvas(tg, new Vector2(mPos.x - tg.originOffset.x, mPos.y - tg.originOffset.y),
                new Vector2(gridOnCanvas.width, gridOnCanvas.height));

            gridOnCanvas.x = mPos.x;
            gridOnCanvas.y = mPos.y;

            FromCanvasPixelToTextureTexel();
            FromTextureTexelToCanvasPixel();      

            if (!isTex)
            {
                RebuildVerticalLines();
                RebuildHorizontalLines();
            }
        }

        /// <summary>
        /// Updates grid data when this item points to a texture pattern.
        /// </summary>
        private void OnTexturePattern()
        {
            isTexPattern = true;
            gridLabel = "Texture Rect";
            gridColumns = 1;
            gridRows = 1;
        }

        /// <summary>
        /// Updates the TextureGrid reference.
        /// </summary>
        public void SetRef(TextureGrid textureGrid)
        {
            tg = textureGrid;
        }

        /// <summary>
        /// Draws the custom grid and its handles.
        /// </summary>
        public void DrawCustomGrid()
        {
            DrawGrid();

            if (isSelected && handlesPos == HandlesPos.OnGridMainRect)
                DrawHandlesOnMainRect();
        }


        /// <summary>
        /// Draws the custom grid.
        /// </summary>
        private void DrawGrid()
        {
            float x = gridOnCanvas.x + tg.originOffset.x;
            float y = gridOnCanvas.y + tg.originOffset.y;
            float width = gridOnCanvas.width;
            float height = gridOnCanvas.height;

            Handles.BeginGUI();

            if (isSelected)
                Handles.color = Color.yellow;
            else
                Handles.color = gridColor;

            if (isTexPattern)
            {
                float discRadius = 4.0f;

                if (isOverlapping)
                    Handles.color = Color.red;

                Handles.DrawSolidDisc(new Vector3(x, y), new Vector3(0, 0, 1), discRadius);
                Handles.DrawSolidDisc(new Vector3(x + width, y), new Vector3(0, 0, 1), discRadius);
                Handles.DrawSolidDisc(new Vector3(x + width, y + height), new Vector3(0, 0, 1), discRadius);
                Handles.DrawSolidDisc(new Vector3(x, y + height), new Vector3(0, 0, 1), discRadius);

                Handles.DrawLine(new Vector3(x, y), new Vector3(x + width, y));
                Handles.DrawLine(new Vector3(x + width, y), new Vector3(x + width, y + height));
                Handles.DrawLine(new Vector3(x + width, y + height), new Vector3(x, y + height));
                Handles.DrawLine(new Vector3(x, y + height), new Vector3(x, y));         
            }

            if (!isTexPattern)
            {
                if (handlesPos == HandlesPos.OnVerticalLines)
                {
                    DrawHorizontalLines(x, y, width);
                    DrawVerticalLines(x, y, height);
                }
                else
                {
                    DrawVerticalLines(x, y, height);
                    DrawHorizontalLines(x, y, width);
                }                    
            }

            Handles.EndGUI();
        }

        /// <summary>
        /// Draws the vertical lines of the grid.
        /// </summary>
        /// <param name="x"> Position of the grid on the X axis. </param>
        /// <param name="y"> Position of the grid on the Y axis. </param>
        /// <param name="height"> Grid height. </param>
        private void DrawVerticalLines(float x, float y, float height)
        {
            for (int i = 0; i < vLinesOnTexGrid.Count; i++)
            {
                if (isSelected)
                {
                    if(handlesPos == HandlesPos.OnVerticalLines && i > 0 && i < vLinesOnTexGrid.Count - 1)
                        Handles.color = new Color(1f, 0.6f, 0f,1);
                    else if(handlesPos == HandlesPos.OnGridMainRect && (i == 0 || i == vLinesOnTexGrid.Count - 1))
                        Handles.color = new Color(1f, 0.6f, 0f, 1);
                    else
                        Handles.color = Color.yellow;
                }
                else
                {
                    Handles.color = gridColor;
                }

                if (isOverlapping)
                    Handles.color = Color.red;

                Handles.DrawLine(new Vector3(x + vLinesOnCanvasGrid[i], y), new Vector3(x + vLinesOnCanvasGrid[i], y + height));

                if (isSelected && handlesPos == HandlesPos.OnVerticalLines)
                {
                    if(i > 0 && i < vLinesOnTexGrid.Count - 1)
                        DrawHandle(new Vector2(x + vLinesOnCanvasGrid[i], y + gridOnCanvas.height * 0.5f), tg.handleSize);
                }
            }
        }

        /// <summary>
        /// Draws the inner horizontal lines of the grid.
        /// </summary>
        /// <param name="x"> Position of the grid on the X axis. </param>
        /// <param name="y"> Position of the grid on the Y axis. </param>
        /// <param name="width"> Grid width. </param>
        private void DrawHorizontalLines(float x, float y, float width)
        {
            for (int i = 0; i < hLinesOnTexGrid.Count; i++)
            {
                if (isSelected)
                {
                    if (handlesPos == HandlesPos.OnHorizontalLines && i > 0 && i < hLinesOnTexGrid.Count - 1)
                        Handles.color = new Color(1f, 0.6f, 0f, 1);
                    else if (handlesPos == HandlesPos.OnGridMainRect && (i == 0 || i == hLinesOnTexGrid.Count - 1))
                        Handles.color = new Color(1f, 0.6f, 0f, 1);
                    else
                        Handles.color = Color.yellow;
                }
                else
                {
                    Handles.color = gridColor;
                }

                if(isOverlapping)
                    Handles.color = Color.red;

                Handles.DrawLine(new Vector3(x, y + hLinesOnCanvasGrid[i]), new Vector3(x + width, y + hLinesOnCanvasGrid[i]));

                if (isSelected && handlesPos == HandlesPos.OnHorizontalLines)
                {
                    if (i > 0 && i < hLinesOnTexGrid.Count - 1)
                        DrawHandle(new Vector2(x + gridOnCanvas.width * 0.5f , y + hLinesOnCanvasGrid[i]), tg.handleSize);
                }
            }
        }

        /// <summary>
        /// Draws the the handles that change the size of the grid.
        /// </summary>
        private void DrawHandlesOnMainRect()
        {
            DrawHandle(new Vector2(gridOnCanvas.x + tg.originOffset.x, gridOnCanvas.y + tg.originOffset.y + gridOnCanvas.height * 0.5f), tg.handleSize);
            DrawHandle(new Vector2(gridOnCanvas.x + tg.originOffset.x + gridOnCanvas.width, gridOnCanvas.y + tg.originOffset.y + gridOnCanvas.height * 0.5f), tg.handleSize);
            DrawHandle(new Vector2(gridOnCanvas.x + tg.originOffset.x + gridOnCanvas.width * 0.5f, gridOnCanvas.y + tg.originOffset.y), tg.handleSize);
            DrawHandle(new Vector2(gridOnCanvas.x + tg.originOffset.x + gridOnCanvas.width * 0.5f, gridOnCanvas.y + tg.originOffset.y + gridOnCanvas.height), tg.handleSize);
        }

        /// <summary>
        /// Draws a custom grid handle.
        /// </summary>
        /// <param name="handlePos"> Handle postion. </param>
        /// <param name="handleSize"> Handle size. </param>
        private void DrawHandle(Vector2 handlePos, int handleSize)
        {
            Rect handleRect = new Rect(handlePos.x - handleSize * 0.5f, handlePos.y - handleSize * 0.5f, handleSize, handleSize);
            GUI.DrawTexture(handleRect, EditorGUIUtility.whiteTexture);
        }

        /// <summary>
        /// Processes the mouse and keyboard events.
        /// </summary>
        /// <param name="e"> A UnityGUI event. </param>
        public void ProcessEvents(Event e)
        {

            switch (e.type)
            {
                case EventType.MouseDown:
                    ProcessMouseDownEvents(e);
                    break;
                case EventType.MouseUp:
                    OnMouseUpEvent();
                    break;
                case EventType.MouseDrag:
                    ProcessMouseDragEvents(e);
                    break;
                case EventType.KeyDown:
                    ProcessKeyDownEvents(e);
                    break;
            }
        }

        /// <summary>
        /// Processes a mouse down event.
        /// </summary>
        /// <param name="e"> A UnityGUI event. </param>
        private void ProcessMouseDownEvents(Event e)
        {
            if (e.button == 0)
            {
                if (MouseInsideRect(gridOnCanvas, -5, e.mousePosition) && e.mousePosition.x > tg.sidePanelWidth)
                {
                    if (!isSelectionLocked || (isSelectionLocked && e.shift))                 
                        SelectGrid(e);            
                }
                else
                {
                    if (e.mousePosition.x > tg.sidePanelWidth)              
                        DeselectGrid(e);           
                }

                if (isSelected)
                {
                    if (handlesPos == HandlesPos.OnGridMainRect)
                        isHandleSelected = IsMouseDownOnGridSizeHandle(e.mousePosition);
                    else if (handlesPos == HandlesPos.OnVerticalLines)
                        isHandleSelected = IsMouseDownOnVerticalLineHandle(e.mousePosition);
                    else
                        isHandleSelected = IsMouseDownOnHorizontalLineHandle(e.mousePosition);
                }
            }

            if (e.button == 1 && isSelected && MouseInsideRect(gridOnCanvas, 0, e.mousePosition))
            {
                DisplayContextMenu(e.mousePosition);
                e.Use();
            }
        }

        /// <summary>
        /// Sets different data when a mouse up event is detected.
        /// </summary>
        private void OnMouseUpEvent()
        {
            isDragged = false;
            isHandleSelected = false;
            handleID = -1;
        }

        /// <summary>
        /// Processes a mouse drag event.
        /// </summary>
        /// <param name="e"> A UnityGUI event. </param>
        private void ProcessMouseDragEvents(Event e)
        {
            if (e.button == 0 && isDragged)
            {
                if (!isHandleSelected && !isGridPosLocked)
                    DragGrid(e.delta, e.mousePosition);

                if (isHandleSelected)
                {
                    if (handlesPos == HandlesPos.OnGridMainRect)
                        ResizeGrid(e.delta, e.mousePosition);
                    else if (handlesPos == HandlesPos.OnVerticalLines)
                        DragVerticalLine(e.delta, e.mousePosition);
                    else
                        DragHorizontalLine(e.delta, e.mousePosition);
                }
                e.Use();
            }
        }

        /// <summary>
        /// Processes a key down event.
        /// </summary>
        /// <param name="e"> A UnityGUI event. </param>
        private void ProcessKeyDownEvents(Event e)
        {
            if (isSelected && !isTexPattern)
            {
                switch (e.keyCode)
                {
                    case KeyCode.UpArrow:
                        AddHorizontalLine(e);
                        break;
                    case KeyCode.DownArrow:
                        RemoveHorizontalLine(e);
                        break;
                    case KeyCode.RightArrow:
                        AddVerticalLine(e);
                        break;
                    case KeyCode.LeftArrow:
                        RemoveVerticalLine(e);
                        break;
                    case KeyCode.Alpha1:
                        SetHandlesOnGridRect(e);
                        break;
                    case KeyCode.Alpha2:
                        SetHandlesOnVerticalLines(e);
                        break;
                    case KeyCode.Alpha3:
                        SetHandlesOnHorizontalLines(e);
                        break;
                }
            }
        }

        /// <summary>
        /// Changes the handles position when a key is pressed. The handles change the grid size.
        /// </summary>
        /// <param name="e"> A UnityGUI event. </param>
        private void SetHandlesOnGridRect(Event e)
        {
            handlesPos = HandlesPos.OnGridMainRect;
            GUI.FocusControl(null);
            GUI.changed = true;
            e.Use();
        }

        /// <summary>
        /// Changes the handles position.
        /// </summary>
        /// <param name="e"> A UnityGUI event. </param>
        private void SetHandlesOnVerticalLines(Event e)
        {
            if (gridColumns > 1) {
                handlesPos = HandlesPos.OnVerticalLines;
                GUI.FocusControl(null);
                GUI.changed = true;
                e.Use();
            }
        }

        /// <summary>
        /// Changes the handles position.
        /// </summary>
        /// <param name="e"> A UnityGUI event. </param>
        private void SetHandlesOnHorizontalLines(Event e)
        {
            if (gridRows > 1)
            {
                handlesPos = HandlesPos.OnHorizontalLines;
                GUI.FocusControl(null);
                GUI.changed = true;
                e.Use();
            }
        }

        /// <summary>
        /// Updates the grids internal data when the grid is selected in the Grid Editor.
        /// </summary>
        private void SelectGrid(Event e)
        {
            tg.ClearSelection();
            isDragged = true;
            isSelected = true;
            mouseInsideRectPos = new Vector2Int(Mathf.RoundToInt(e.mousePosition.x - gridOnCanvas.x), Mathf.RoundToInt(e.mousePosition.y - gridOnCanvas.y));
            GUI.changed = true;
            e.Use();
        }

        /// <summary>
        /// Updates the grids internal data when the grid is deselected in the Grid Editor.
        /// </summary>
        private void DeselectGrid(Event e)
        {
            DeselectGrid();
            GUI.changed = true;
        }

        /// <summary>
        /// Updates the grids internal data when the grid is deselected in the Grid Editor.
        /// </summary>
        public void DeselectGrid()
        {
            isSelected = false;
            isDragged = false;
            isHandleSelected = false;
            handleID = -1;
        }

        /// <summary>
        /// Increases the rows number by one.
        /// </summary>
        private void AddHorizontalLine(Event e)
        {
            float height = (float)gridHeight / (gridRows + 1);

            if (height >= 3 && !isTexPattern)
            {
                gridRows += 1;
                GUI.FocusControl(null);
                GUI.changed = true;
                e.Use();
                RebuildHorizontalLines();
            }
        }

        /// <summary>
        /// Decreases the rows number by one.
        /// </summary>
        private void RemoveHorizontalLine(Event e)
        {
            if (gridRows > 1 && !isTexPattern)
            {
                gridRows -= 1;
                GUI.FocusControl(null);
                GUI.changed = true;
                e.Use();
                RebuildHorizontalLines();
            }
        }

        /// <summary>
        /// Increases the columns number by one.
        /// </summary>
        private void AddVerticalLine(Event e)
        {
            float width = (float)gridWidth / (gridColumns + 1);

            if (width >= 3 && !isTexPattern)
            {
                gridColumns += 1;
                GUI.FocusControl(null);
                GUI.changed = true;
                e.Use();
                RebuildVerticalLines();
            }
        }

        /// <summary>
        /// Decreases the columns number by one.
        /// </summary>
        private void RemoveVerticalLine(Event e)
        {
            if (gridColumns > 1 && !isTexPattern)
            {
                gridColumns -= 1;
                GUI.FocusControl(null);
                GUI.changed = true;
                e.Use();
                RebuildVerticalLines();
            }
        }

        /// <summary>
        /// Rebuilds the verticals lines data.
        /// </summary>
        public void RebuildVerticalLines()
        {
            float colWidth = (float)gridWidth / gridColumns;

            vLinesOnTexGrid.Clear();
            vLinesOnCanvasGrid.Clear();

            for (int i = 0; i < gridColumns + 1; i++)
            {
                vLinesOnTexGrid.Add(Mathf.CeilToInt(i * colWidth));
                vLinesOnCanvasGrid.Add(FromTextureGridToCanvasGrid(Mathf.CeilToInt(i * colWidth), true));
            }
        }

        /// <summary>
        /// Rebuilds the horizontal lines data.
        /// </summary>
        public void RebuildHorizontalLines()
        {
            float rowHeight = (float)gridHeight / gridRows;

            hLinesOnTexGrid.Clear();
            hLinesOnCanvasGrid.Clear();

            for (int i = 0; i < gridRows + 1; i++)
            {
                hLinesOnTexGrid.Add(Mathf.CeilToInt(i * rowHeight));
                hLinesOnCanvasGrid.Add(FromTextureGridToCanvasGrid(Mathf.CeilToInt(i * rowHeight), false));
            }
        }

        /// <summary>
        /// Determines if the mouse arrow is inside one of the handles that control the size of the grid.
        /// </summary>
        /// <param name="mousePosition"> Current position of the mouse. </param>
        private bool IsMouseDownOnGridSizeHandle(Vector2 mousePosition)
        {
            Rect handleRect;

            // Left handle
            handleRect = new Rect(gridOnCanvas.x, gridOnCanvas.y + gridOnCanvas.height * 0.5f, tg.handleSize, tg.handleSize);
            if(MouseInsideRect(handleRect, -8, mousePosition)){
                handleID = 3;
                return true;
            }

            // Right handle
            handleRect = new Rect(gridOnCanvas.x + gridOnCanvas.width, gridOnCanvas.y + gridOnCanvas.height * 0.5f, tg.handleSize, tg.handleSize);
            if (MouseInsideRect(handleRect, -8, mousePosition))
            {
                handleID = 1;
                return true;
            }

            // Top handle
            handleRect = new Rect(gridOnCanvas.x + gridOnCanvas.width * 0.5f, gridOnCanvas.y, tg.handleSize, tg.handleSize);
            if (MouseInsideRect(handleRect, -8, mousePosition))
            {
                handleID = 0;
                return true;
            }

            // Bottom handle
            handleRect = new Rect(gridOnCanvas.x + gridOnCanvas.width * 0.5f, gridOnCanvas.y + gridOnCanvas.height, tg.handleSize, tg.handleSize);
            if (MouseInsideRect(handleRect, -8, mousePosition))
            {
                handleID = 2;
                return true;
            }
       
            return false;
        }

        /// <summary>
        /// Determines if the mouse arrow is inside one of the handles that control the position of a vertical line.
        /// </summary>
        /// <param name="mousePosition"> Current position of the mouse in widow space. </param>
        /// <returns> Returns true if the mouse arrow is inside a handle, otherwise returns false. </returns>
        private bool IsMouseDownOnVerticalLineHandle(Vector2 mousePosition)
        {
            Rect handleRect;

            for (int i = 1; i < vLinesOnCanvasGrid.Count - 1; i++)
            {
                handleRect = new Rect(gridOnCanvas.x + vLinesOnCanvasGrid[i], gridOnCanvas.y + gridOnCanvas.height * 0.5f, tg.handleSize, tg.handleSize);

                if (MouseInsideRect(handleRect, -8, mousePosition))
                {
                    handleID = i;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines if the mouse arrow is inside one of the handles that control the position of a horizontal line.
        /// </summary>
        /// <param name="mousePosition"> Current position of the mouse in widow space. </param>
        /// <returns> Returns true if the mouse arrow is inside a handle, otherwise returns false. </returns>
        private bool IsMouseDownOnHorizontalLineHandle(Vector2 mousePosition)
        {
            Rect handleRect;

            for (int i = 1; i < hLinesOnCanvasGrid.Count - 1; i++)
            {
                handleRect = new Rect(gridOnCanvas.x + gridOnCanvas.width * 0.5f, gridOnCanvas.y + hLinesOnCanvasGrid[i], tg.handleSize, tg.handleSize);

                if (MouseInsideRect(handleRect, -8, mousePosition))
                {
                    handleID = i;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Offsets the position of the grid.
        /// </summary>
        /// <param name="delta"> Position offset. </param>
        /// <param name="mPos"> Mouse position. </param>
        private void DragGrid(Vector2 delta, Vector2 mPos)
        {
            Vector2 pos;

            if (mPos.x > tg.sidePanelWidth)
            {
                if (isSelected && !isHandleSelected)
                {
                    pos = mPos - mouseInsideRectPos;
                    pos = TG_Utils.ClampPosToCanvas(tg, pos, new Vector2(gridOnCanvas.width, gridOnCanvas.height));

                    gridOnCanvas.x = pos.x;
                    gridOnCanvas.y = pos.y;
                }
            }

            FromCanvasPixelToTextureTexel();
            FromTextureTexelToCanvasPixel();
        }


        /// <summary>
        /// Updates the position of the vertical and horizontal lines when the grid is resized.
        /// </summary>
        private void UpdateGridLinesPos()
        {
            float colWidth = (float)gridWidth / gridColumns;
            float rowHeight = (float)gridHeight / gridRows;

            for (int i = 0; i < vLinesOnTexGrid.Count; i++)
            {
                vLinesOnTexGrid[i] = Mathf.CeilToInt(i * colWidth);
                vLinesOnCanvasGrid[i] = FromTextureGridToCanvasGrid(vLinesOnTexGrid[i], true);
            }

            for (int i = 0; i < hLinesOnTexGrid.Count; i++)
            {
                hLinesOnTexGrid[i] = Mathf.CeilToInt(i * rowHeight);
                hLinesOnCanvasGrid[i] = FromTextureGridToCanvasGrid(hLinesOnTexGrid[i], false);
            }
        }

        /// <summary>
        /// Updates the positions of the vertical lines.
        /// </summary>
        public void UpdateVerticalLinesPos()
        {
            float colWidth = (float)gridWidth / gridColumns;

            for (int i = 0; i < vLinesOnTexGrid.Count; i++)
            {
                vLinesOnTexGrid[i] = Mathf.CeilToInt(i * colWidth);
                vLinesOnCanvasGrid[i] = FromTextureGridToCanvasGrid(vLinesOnTexGrid[i], true);
            }
        }

        /// <summary>
        /// Updates the positions of the horizontal lines.
        /// </summary>
        public void UpdateHorizontalLinesPos()
        {
            float rowHeight = (float)gridHeight / gridRows;

            for (int i = 0; i < hLinesOnTexGrid.Count; i++)
            {
                hLinesOnTexGrid[i] = Mathf.CeilToInt(i * rowHeight);
                hLinesOnCanvasGrid[i] = FromTextureGridToCanvasGrid(hLinesOnTexGrid[i], false);
            }
        }

        /// <summary>
        /// Used to determine if the mouse arrow is inside a rectangle.
        /// </summary>
        /// <param name="rect"> A rectrangle. </param>
        /// <param name="sizeOffset"> Rectangle size offset. </param>
        /// <param name="mousePosition"> Mouse position. </param>
        /// <returns> Returns true if the mouse arrow is inside the rect, otherwise returns false. </returns>
        private bool MouseInsideRect(Rect rect, int sizeOffset, Vector2 mousePosition)
        {
            rect.x += tg.originOffset.x + sizeOffset;
            rect.y += tg.originOffset.y + sizeOffset;

            if (new Rect(rect.x, rect.y , rect.width - sizeOffset * 2, rect.height - sizeOffset * 2).Contains(mousePosition))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Offsets the handles that control the grids size.
        /// </summary>
        /// <param name="delta"> Position offset. </param>
        /// <param name="mPos"> Mouse position. </param>
        private void ResizeGrid(Vector2 delta, Vector2 mPos)
        {
            float dist;
            Vector2 pos;

            if (handleID == 0 || handleID == 2)
                dist = (float)(tg.texAtlas.height + tg.sizeOffset.y) / tg.texAtlas.height * 3;
            else
                dist = (float)(tg.texAtlas.width + tg.sizeOffset.x) / tg.texAtlas.width * 3;

            pos = mPos - tg.originOffset;
            pos = TG_Utils.ClampPosToCanvas(tg, pos, Vector2.zero);

            if (handleID == 0)
            {
                float bot = gridOnCanvas.y + gridOnCanvas.height;

                if (gridRows > 1)
                {
                    if ((bot - pos.y) / gridRows < dist)
                        pos.y = bot - gridRows * dist;
                }
                else
                {
                    if (bot - pos.y < dist)
                        pos.y = bot - dist;
                }

                gridOnCanvas.y = pos.y;
                gridOnCanvas.height = bot - pos.y;
            }
            else if(handleID == 1)
            {
                if(gridColumns > 1)
                {
                    if ((pos.x - gridOnCanvas.x) / gridColumns < dist)
                        pos.x = gridOnCanvas.x + gridColumns * dist;
                }
                else
                {
                    if (gridOnCanvas.x + dist > pos.x)
                        pos.x = gridOnCanvas.x + dist;
                }

                gridOnCanvas.width = pos.x - gridOnCanvas.x;
            }
            else if(handleID == 2)
            {
                if(gridRows > 1)
                {
                    if ((pos.y - gridOnCanvas.y) / gridRows < dist)
                        pos.y = gridOnCanvas.y + gridRows * dist;
                }
                else
                {
                    if (pos.y - gridOnCanvas.y < dist)
                        pos.y = gridOnCanvas.y + dist;
                }

                gridOnCanvas.height = pos.y - gridOnCanvas.y;
            }
            else
            {
                float righ = gridOnCanvas.x + gridOnCanvas.width;

                if(gridColumns > 1)
                {
                    if ((righ - pos.x) / gridColumns < dist)
                        pos.x = righ - gridColumns * dist;
                }
                else
                {
                    if (righ - pos.x < dist)
                        pos.x = righ - dist;
                }

                gridOnCanvas.x = pos.x;
                gridOnCanvas.width = righ - pos.x;
            }

            FromCanvasPixelToTextureTexel();
            FromTextureTexelToCanvasPixel();
            UpdateGridLinesPos();
        }

        /// <summary>
        /// Offsets the position of a vertical line on X axis.
        /// </summary>
        /// <param name="delta"> Offset to apply. </param>
        /// <param name="mPos"> Mouse position in window space. </param>
        private void DragVerticalLine(Vector2 delta, Vector2 mPos)
        {
            float minX = 0, maxX = 0, posX;

            minX = (int)gridOnCanvas.x + vLinesOnCanvasGrid[handleID - 1];
            maxX = (int)gridOnCanvas.x + vLinesOnCanvasGrid[handleID + 1];

            posX = mPos.x - tg.originOffset.x;

            float xScale = (float)(tg.texAtlas.width + tg.sizeOffset.x) / tg.texAtlas.width;
            float space = 3 * xScale;

            if (posX < minX + space)
                posX = minX + space;

            if (posX > maxX - space)
                posX = maxX - space;

            vLinesOnCanvasGrid[handleID] = posX - gridOnCanvas.x;

            vLinesOnTexGrid[handleID] = FromCanvasGridToTextureGrid(vLinesOnCanvasGrid[handleID], true);
            vLinesOnCanvasGrid[handleID] = FromTextureGridToCanvasGrid(vLinesOnTexGrid[handleID], true);
        }

        /// <summary>
        /// Offsets the position of a horizontal line on Y axis.
        /// </summary>
        /// <param name="delta"> Offset to apply. </param>
        /// <param name="mPos"> Mouse position in window space. </param>
        private void DragHorizontalLine(Vector2 delta, Vector2 mPos)
        {
            float minY, maxY, posY;

            minY = (int)gridOnCanvas.y + hLinesOnCanvasGrid[handleID + 1];
            maxY = (int)gridOnCanvas.y + hLinesOnCanvasGrid[handleID - 1];

            posY = mPos.y - tg.originOffset.y;

            float yScale = (float)(tg.texAtlas.height + tg.sizeOffset.y) / tg.texAtlas.height;
            float space = 3 * yScale;

            if (posY < minY + space)
                posY = minY + space;

            if (posY > maxY - space)
                posY = maxY - space;

            hLinesOnCanvasGrid[handleID] = posY - gridOnCanvas.y;

            hLinesOnTexGrid[handleID] = FromCanvasGridToTextureGrid(hLinesOnCanvasGrid[handleID], false);
            hLinesOnCanvasGrid[handleID] = FromTextureGridToCanvasGrid(hLinesOnTexGrid[handleID], false);
        }

        /// <summary>
        /// Conversts a 1D value from a texture space grid to a canvas space grid.
        /// </summary>
        /// <param name="pos"> Position on X or Y axis in a grid. </param>
        /// <param name="isVerticalLine"> Used to determine if pos belongs to a vertical line. </param>
        /// <returns> The position on a grid that exists in the canvas space. </returns>
        private float FromTextureGridToCanvasGrid(int pos, bool isVerticalLine)
        {
            float posOnCanvasGrid, newSize, uv;

            if (isVerticalLine)
            {
                uv = (float)(pos + gridPos.x)/ tg.texAtlas.width;
                newSize = tg.texAtlas.width + tg.sizeOffset.x;
                posOnCanvasGrid = newSize * uv - gridOnCanvas.x;
            }
            else
            {
                uv = (float)((tg.texAtlas.height - (gridPos.y + pos))) / tg.texAtlas.height;
                newSize = tg.texAtlas.height + tg.sizeOffset.y;
                posOnCanvasGrid = newSize * uv - gridOnCanvas.y;
            }

            return posOnCanvasGrid;
        }

        /// <summary>
        /// Conversts a 1D value from a canvas space grid to a texture space grid.
        /// </summary>
        /// <param name="pos"> Position on X or Y axis in a grid. </param>
        /// <param name="isVerticalLine"> Used to determine if pos belongs to a vertical line. </param>
        /// <returns> The position on a grid that exists in the texture space. </returns>
        private int FromCanvasGridToTextureGrid(float pos, bool isVerticalLine)
        {
            float size, uv;
            int posOnTexGrid;

            if (isVerticalLine)
            {         
                size = tg.texAtlas.width + tg.sizeOffset.x;
                uv = (pos + gridOnCanvas.x) / size;
                posOnTexGrid = Mathf.CeilToInt(uv * tg.texAtlas.width);
                posOnTexGrid -= gridPos.x;
            }
            else
            {
                size = tg.texAtlas.height + tg.sizeOffset.y;
                uv = (pos + gridOnCanvas.y) / size;

                posOnTexGrid = Mathf.CeilToInt(uv * tg.texAtlas.height);
                posOnTexGrid = tg.texAtlas.height - posOnTexGrid;
                posOnTexGrid -= gridPos.y;
            }

            return posOnTexGrid;
        }

        /// <summary>
        /// Converts the grids position, width and height from texture space to canvas space.
        /// </summary>
        public void FromTextureTexelToCanvasPixel()
        {
            Vector2 gPos = new Vector2(gridPos.x, gridPos.y);
            gPos.y = tg.texAtlas.height - gPos.y - gridHeight;

            Vector2 uvSpacePos1 = new Vector2((float)gPos.x / tg.texAtlas.width, (float)gPos.y / tg.texAtlas.height);
            Vector2 uvSpacePos2 = new Vector2((float)(gPos.x + gridWidth) / tg.texAtlas.width, (float)(gPos.y + gridHeight) / tg.texAtlas.height);

            int newWidth = tg.texAtlas.width + tg.sizeOffset.x;
            int newHeight = tg.texAtlas.height + tg.sizeOffset.y;

            Vector2 screenSpacePos1 = new Vector2(newWidth * uvSpacePos1.x, newHeight * uvSpacePos1.y);
            Vector2 screenSpacePos2 = new Vector2(newWidth * uvSpacePos2.x, newHeight * uvSpacePos2.y);

            gridOnCanvas = new Rect(screenSpacePos1.x, screenSpacePos1.y, screenSpacePos2.x - screenSpacePos1.x, screenSpacePos2.y - screenSpacePos1.y);      
        }

        /// <summary>
        /// Converts the grids position, width and height from canvas space to texture space.
        /// </summary>
        private void FromCanvasPixelToTextureTexel()
        {
            Rect cgRect = new Rect(gridOnCanvas.x, gridOnCanvas.y, gridOnCanvas.width, gridOnCanvas.height);

            float newWidth = tg.texAtlas.width + tg.sizeOffset.x;
            float newHeight = tg.texAtlas.height + tg.sizeOffset.y;

            Vector2 uvSpacePos1 = new Vector2(cgRect.x / newWidth, cgRect.y / newHeight);
            Vector2 uvSpacePos2 = new Vector2((cgRect.x + cgRect.width) / newWidth, (cgRect.y + cgRect.height) / newHeight);

            Vector2 texelPos1 = new Vector2(tg.texAtlas.width * uvSpacePos1.x, tg.texAtlas.height * uvSpacePos1.y);
            Vector2 texelPos2 = new Vector2(tg.texAtlas.width * uvSpacePos2.x, tg.texAtlas.height * uvSpacePos2.y);

            cgRect = new Rect(texelPos1.x, texelPos1.y, texelPos2.x - texelPos1.x, texelPos2.y - texelPos1.y);

            gridPos.x = Mathf.RoundToInt(cgRect.x);
            gridPos.y = Mathf.RoundToInt(cgRect.y);
            gridWidth = Mathf.RoundToInt(cgRect.width);
            gridHeight = Mathf.RoundToInt(cgRect.height);

            gridPos.y = tg.texAtlas.height - gridPos.y - gridHeight;
        }

        /// <summary>
        /// Converts the column and row line positions from texture grid space to canvas grid space.
        /// </summary>
        public void FromTextureGridsToCanvasGrids()
        {
            for (int i = 0; i < vLinesOnTexGrid.Count; i++)
            {
                vLinesOnCanvasGrid[i] = FromTextureGridToCanvasGrid(vLinesOnTexGrid[i], true);
            }

            for (int i = 0; i < hLinesOnTexGrid.Count; i++)
            {
                hLinesOnCanvasGrid[i] = FromTextureGridToCanvasGrid(hLinesOnTexGrid[i], false);
            }
        }

        /// <summary>
        /// Creates a context menu for the current grid.
        /// </summary>
        /// <param name="mousePosition"> Mouse position. </param>
        private void DisplayContextMenu(Vector2 mousePosition)
        {
            GenericMenu genericMenu = new GenericMenu();

            if (!isTexPattern)
            {
                genericMenu = DisplayAddLineOptions(genericMenu, mousePosition);
                genericMenu = DisplayRemoveLineOptions(genericMenu, mousePosition);
                genericMenu = DisplayHandleOptions(genericMenu);
            }
    
            genericMenu = DisplayLockOptions(genericMenu);
            genericMenu.AddSeparator("");
            genericMenu.AddItem(new GUIContent("Copy"), false, () => tg.CopyCustomGrid(this));
            genericMenu.AddItem(new GUIContent("Delete"), false, () => tg.RemoveGrid(this));
            genericMenu.ShowAsContext();

        }

        //private void DeleteThisGrid()
        //{
        //    tg.RemoveGrid(this);
        //}

        /// <summary>
        /// Adds the option to add a vertical or horizontal line to the grid, in the context menu.
        /// </summary>
        /// <param name="genericMenu"> Reference to a generic menu. </param>
        /// <param name="mousePosition"> Mouse position. </param>
        private GenericMenu DisplayAddLineOptions(GenericMenu genericMenu, Vector2 mousePosition)
        {
            genericMenu.AddItem(new GUIContent("Add Vertical Line"), false, () => AddVerticalLine(mousePosition));
            genericMenu.AddItem(new GUIContent("Add Horizontal Line"), false, () => AddHorizontalLine(mousePosition));

            return genericMenu;
        }

        /// <summary>
        /// Adds the option to remove a vertical or horizontal line from the grid, in the context menu.
        /// </summary>
        /// <param name="genericMenu"> Reference to a generic menu. </param>
        /// <param name="mousePosition"> Mouse position. </param>
        private GenericMenu DisplayRemoveLineOptions(GenericMenu genericMenu, Vector2 mousePosition)
        {

            bool isMouseOnVerticalLine = false;
            bool isMouseOnHorizontalLine = false;
            int cIndex, rIndex;

            isMouseOnVerticalLine = IsMouseOnVerticalLine(mousePosition, out cIndex);
            isMouseOnHorizontalLine = IsMouseOnHorizontalLine(mousePosition, out rIndex);

            genericMenu.AddSeparator("");

            if (isMouseOnVerticalLine && gridColumns > 1)
                genericMenu.AddItem(new GUIContent("Remove Vertical Line"), false, () => RemoveVerticalLineAt(cIndex));
            else
                genericMenu.AddDisabledItem(new GUIContent("Remove Vertical Line"));

            if (isMouseOnHorizontalLine && gridRows > 1)
                genericMenu.AddItem(new GUIContent("Remove Horizontal Line"), false, () => RemoveHorizontalLineAt(rIndex));
            else
                genericMenu.AddDisabledItem(new GUIContent("Remove Horizontal Line"));

            return genericMenu;
        }

        /// <summary>
        /// Adds the options that control the handles position on the grid, in the context menu.
        /// </summary>
        /// <param name="genericMenu"> Reference to a generic menu. </param>
        private GenericMenu DisplayHandleOptions(GenericMenu genericMenu)
        {
            genericMenu.AddSeparator("");

            if (handlesPos == HandlesPos.OnGridMainRect)
                genericMenu.AddItem(new GUIContent("Handles/On Main Rect _1"), true, SetHandlesOnGridMainRect);
            else
                genericMenu.AddItem(new GUIContent("Handles/On Main Rect _1"), false, SetHandlesOnGridMainRect);

            if (handlesPos == HandlesPos.OnVerticalLines)
                genericMenu.AddItem(new GUIContent("Handles/On Vertical Lines _2"), true, SetHandlesOnVerticalLines);
            else
                genericMenu.AddItem(new GUIContent("Handles/On Vertical Lines _2"), false, SetHandlesOnVerticalLines);

            if (handlesPos == HandlesPos.OnHorizontalLines)
                genericMenu.AddItem(new GUIContent("Handles/On Horizontal Lines _3"), true, SetHandlesOnHorizontalLines);
            else
                genericMenu.AddItem(new GUIContent("Handles/On Horizontal Lines _3"), false, SetHandlesOnHorizontalLines);
            genericMenu.AddSeparator("");
            return genericMenu;
        }

        /// <summary>
        /// Adds the lock options, in the context menu.
        /// </summary>
        /// <param name="genericMenu"> Reference to a generic menu. </param>
        private GenericMenu DisplayLockOptions(GenericMenu genericMenu)
        {
            if (isGridPosLocked)
                genericMenu.AddItem(new GUIContent("Lock Position"), true, () => OnLockUnlockGridPosClick(false));
            else
                genericMenu.AddItem(new GUIContent("Lock Position"), false, () => OnLockUnlockGridPosClick(true));

            if (isSelectionLocked)
                genericMenu.AddItem(new GUIContent("Lock Selection"), true, OnUnlockSelectionClick);
            else
                genericMenu.AddItem(new GUIContent("Lock Selection"), false, OnLockSelectionClick);

            return genericMenu;
        }

        /// <summary>
        /// Lock/Unlocks the ability to drag a grid on the canvas.
        /// </summary>
        /// <param name="isLocked"> The state of the grid. </param>
        private void OnLockUnlockGridPosClick(bool isLocked)
        {
            isGridPosLocked = isLocked;
        }

        /// <summary>
        /// Removes a vertical line from the current grid.
        /// </summary>
        /// <param name="index"> Index of the line that needs to be deleted. </param>
        private void RemoveVerticalLineAt(int index)
        {
            vLinesOnTexGrid.RemoveAt(index);
            vLinesOnCanvasGrid.RemoveAt(index);
            gridColumns--;
            GUI.changed = true;
        }

        /// <summary>
        /// Removes a horizontal line from the current grid.
        /// </summary>
        /// <param name="index"> Index of the line that needs to be deleted. </param>
        private void RemoveHorizontalLineAt(int index)
        {
            hLinesOnTexGrid.RemoveAt(index);
            hLinesOnCanvasGrid.RemoveAt(index);
            gridRows--;
            GUI.changed = true;
        }

        /// <summary>
        /// Checks whether the mouse arrow is above a vertical line of the grid.
        /// </summary>
        /// <param name="mousePosition"> Mouse position in window space. </param>
        /// <param name="vLineIndex"> Index of a vertical line. </param>
        /// <returns> Returns true if the mouse arrow is above a vertical line, otherwise returns false </returns>
        private bool IsMouseOnVerticalLine(Vector2 mousePosition, out int vLineIndex)
        {
            float minX, maxX;
            vLineIndex = -1;
            mousePosition -= tg.originOffset;

            for (int i = 1; i < vLinesOnCanvasGrid.Count - 1; i++)
            {
                minX = gridOnCanvas.x + vLinesOnCanvasGrid[i] - 5;
                maxX = gridOnCanvas.x + vLinesOnCanvasGrid[i] + 5;

                if (mousePosition.x > minX && mousePosition.x < maxX)
                {
                    vLineIndex = i;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks whether the mouse arrow is above a horizntal line of the grid.
        /// </summary>
        /// <param name="mousePosition"> Mouse position in window space. </param>
        /// <param name="hLineIndex"> Index of a horizontal line. </param>
        /// <returns> Returns true if the mouse arrow is above a horizontal line, otherwise returns false </returns>
        private bool IsMouseOnHorizontalLine(Vector2 mousePosition, out int hLineIndex)
        {
            float minY, maxY;
            hLineIndex = -1;
            mousePosition -= tg.originOffset;

            for (int i = 1; i < hLinesOnCanvasGrid.Count - 1; i++)
            {
                minY = gridOnCanvas.y + hLinesOnCanvasGrid[i] - 5;
                maxY = gridOnCanvas.y + hLinesOnCanvasGrid[i] + 5;

                if (mousePosition.y > minY && mousePosition.y < maxY)
                {
                    hLineIndex = i;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Adds a vertical line to the grid.
        /// </summary>
        /// <param name="mousePosition"> Mouse position in window space. </param>
        private void AddVerticalLine(Vector2 mousePosition)
        {
            float minX, maxX;

            mousePosition -= tg.originOffset;
            
            for (int i = 0; i < vLinesOnCanvasGrid.Count - 1; i++)
            {
                minX = gridOnCanvas.x + vLinesOnCanvasGrid[i];
                maxX = gridOnCanvas.x + vLinesOnCanvasGrid[i + 1];

                if (mousePosition.x > minX && mousePosition.x < maxX && (vLinesOnTexGrid[i + 1] - vLinesOnTexGrid[i]) > 5)
                {
                    int pos = vLinesOnTexGrid[i] + Mathf.RoundToInt((vLinesOnTexGrid[i + 1] - vLinesOnTexGrid[i]) * 0.5f);
                    vLinesOnTexGrid.Insert(i + 1, pos);
                    vLinesOnCanvasGrid.Insert(i + 1, FromTextureGridToCanvasGrid(pos, true));
                    gridColumns++;
                    GUI.changed = true;
                    break;
                }
            }
        }

        /// <summary>
        /// Adds a horizontal line to the grid.
        /// </summary>
        /// <param name="mousePosition"> Mouse position in window space. </param>
        private void AddHorizontalLine(Vector2 mousePosition)
        {
            float minY, maxY;

            mousePosition -= tg.originOffset;

            for (int i = 0; i < hLinesOnCanvasGrid.Count - 1; i++)
            {
                minY = gridOnCanvas.y + hLinesOnCanvasGrid[i + 1];
                maxY = gridOnCanvas.y + hLinesOnCanvasGrid[i];

                if (mousePosition.y > minY && mousePosition.y < maxY && (hLinesOnTexGrid[i + 1] - hLinesOnTexGrid[i]) > 5)
                {
                    int pos = hLinesOnTexGrid[i] + Mathf.RoundToInt((hLinesOnTexGrid[i + 1] - hLinesOnTexGrid[i]) * 0.5f);
                    hLinesOnTexGrid.Insert(i + 1, pos);
                    hLinesOnCanvasGrid.Insert(i + 1, FromTextureGridToCanvasGrid(pos, false));
                    gridRows++;
                    GUI.changed = true;
                    break;
                }
            }
        }

        private void OnLockSelectionClick()
        {
            isSelectionLocked = true;
        }

        private void OnUnlockSelectionClick()
        {
            isSelectionLocked = false;
        }

        private void SetHandlesOnVerticalLines()
        {
            handlesPos = HandlesPos.OnVerticalLines;
        }

        private void SetHandlesOnHorizontalLines()
        {
            handlesPos = HandlesPos.OnHorizontalLines;
        }

        private void SetHandlesOnGridMainRect()
        {
            handlesPos = HandlesPos.OnGridMainRect;
        }
#endif
    }
}
