using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif 
using UnityEngine;

namespace Nicrom.PM {
    /// <summary>
    /// Grid color modes.  
    /// </summary>
    public enum GridColorMode {
        /// <summary>
        /// A random color is applied to each grid.  
        /// </summary>
        RandomColor,
        /// <summary>
        /// The same color is applied to all the grids.  
        /// </summary>
        SingleColor
    }

    //public partial class TextureGrid : ScriptableObject {
    public class TextureGrid : ScriptableObject {
        public GridColorMode gridColorMode = GridColorMode.SingleColor;
        /// <summary>
        /// Color to apply to all the grids.  
        /// </summary>
        public Color gridColor = Color.white;
        /// <summary>
        /// List of grids.  
        /// </summary>
        public List<CustomGrid> gridsList = new List<CustomGrid>();
        /// <summary>
        /// List that stores a CustomGrid copy.  
        /// </summary>
        public List<CustomGrid> copyList = new List<CustomGrid>();
        /// <summary>
        /// Reference to a texture atlas.
        /// </summary>
        public Texture2D texAtlas;
        /// <summary>
        /// Instance of a Texture2D that stores the original colors of the texture atlas.
        /// </summary>
        public Texture2D originTexAtlas; 
        /// <summary>
        /// Canvas origin offset.
        /// </summary>
        public Vector2 originOffset = new Vector2(0, 0);
        /// <summary>
        /// Canvas size offset.
        /// </summary>
        public Vector2Int sizeOffset = new Vector2Int(0, 0);
        /// <summary>
        /// Used to determine if the shortcut info should be displayed. 
        /// </summary>
        public bool showShortcuts = false;
        /// <summary>
        /// Canvas zoom speed.
        /// </summary>
        public int zoomSpeed = 80;
        /// <summary>
        /// Grid handle size.
        /// </summary>
        public int handleSize = 10;
        /// <summary>
        /// Canvas boder size.
        /// </summary>
        public int canvasBorder = 50;
        /// <summary>
        /// The default width of the side panel.
        /// </summary>
        public int panelDefaultWidth = 250;
        /// <summary>
        /// The current width of the side panel.
        /// </summary>
        public int sidePanelWidth = 250;
        /// <summary>
        /// Used to determine if the side panel should be drawn.
        /// </summary>
        public bool showPanel = true;
        /// <summary>
        /// Used to determine if the X and Y axes should be drawn in the viewport.
        /// </summary>
        public bool drawAxes = true;
        /// <summary>
        /// Used to determine if the background grid should be drawn in the viewport.
        /// </summary>
        public bool drawGrid = true;
        /// <summary>
        /// The width and height of the texture atlas in pixels.
        /// </summary>
        public Vector2Int texAtlasSize;

#if UNITY_EDITOR
        /// <summary>
        /// Makes sure the CustomGrid objects stored in the gridsList have a reference to this TextureGrid.
        /// </summary>
        public void SetRef()
        {
            for (int i = 0; i < gridsList.Count; i++)
            {
                gridsList[i].SetRef(this);
            }
        }

        /// <summary>
        /// Deselects the selected grid.
        /// </summary>
        public void ClearSelection()
        {
            int count = gridsList.Count;

            for (int i = 0; i < count; i++)
            {
                if (gridsList[i].isSelected)
                {
                    gridsList[i].DeselectGrid();
                    GUI.changed = true;
                    break;
                }
            }
        }

        /// <summary>
        /// Resets the tint colors.
        /// </summary>
        public void ClearTintColors()
        {
            int count = gridsList.Count;

            for (int i = 0; i < count; i++)
            {
                if (gridsList[i].isTexPattern)
                {
                    gridsList[i].tintColor = Color.white;
                }         
            }
        }

        /// <summary>
        /// Creates a Texture2D instance that stores the original colors of the texture atlas.
        /// </summary>
        public void GetOriginalTextureColors()
        {
            Color32[] currentTexColors = texAtlas.GetPixels32();

            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(texAtlas), ImportAssetOptions.ForceUpdate);
            originTexAtlas = new Texture2D(texAtlas.width, texAtlas.height, texAtlas.format, false, true);
            Color32[] origTexColors = texAtlas.GetPixels32();

            originTexAtlas.SetPixels32(origTexColors);
            originTexAtlas.Apply();

            texAtlas.SetPixels32(currentTexColors);
            texAtlas.Apply();
        }

        /// <summary>
        /// Creates a copy of a CustomGrid item.
        /// </summary>
        /// <param name="cg"> Reference to a CustomGrid. </param>
        public void CopyCustomGrid(CustomGrid cg)
        {
            copyList.Clear();
            CustomGrid copy = new CustomGrid(this, cg, Vector2.zero, false);
            copyList.Add(copy);
        }

        /// <summary>
        /// Removed a CustomGrid instance from the gridsList.
        /// </summary>
        /// <param name="cg"> Reference to a CustomGrid. </param>
        public void RemoveGrid(CustomGrid cg)
        {
            gridsList.Remove(cg);
        }
#endif
    }
}
