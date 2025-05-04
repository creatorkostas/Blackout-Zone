#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections.Generic;

namespace Nicrom.PM {
    public static class PM_Utils {
#if UNITY_EDITOR
        /// <summary>
        /// Used to determine if a texture is readable.
        /// </summary>
        /// <param name="texture"> Reference to a texture. </param>
        /// <returns> Returns true if texture is readable, otherwise returns false. </returns>
        public static bool IsTextureReadable(Texture2D texture)
        {
#if UNITY_2018_3_OR_NEWER
          if(texture.isReadable)
                return true;
            else
                return false;
#else
            string texturePath = AssetDatabase.GetAssetPath(texture);
            TextureImporter textureImporter = (TextureImporter)AssetImporter.GetAtPath(texturePath);

            if (textureImporter.isReadable)
                return true;
            else
                return false;
#endif
        }

        /// <summary>
        /// Used to determine if a texture has ARGB32, RGBA32, or RGB24 format.
        /// </summary>
        /// <param name="texture"> Reference to a texture. </param>
        /// <returns> Returns true if the texture has ARGB32, RGBA32, or RGB24 format, otherwise returns false. </returns>
        public static bool HasSuportedTextureFormat(Texture2D texture)
        {
            if (texture.format == TextureFormat.ARGB32 || texture.format == TextureFormat.RGBA32 || texture.format == TextureFormat.RGB24)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Calculates the position, width and height of a custom grid cell.
        /// </summary>
        /// <param name="pMod"> Reference to a PaletteModifier script. </param>
        /// <param name="point"> The coordinates of a texture texel. </param>
        /// <param name="isPointInsideCG"> Used to determine if a point in inside a custom grid. </param>
        /// <param name="isTexPattern"> Used to determine if the custom grid points to a texture pattern on the texture atlas. </param>
        /// <returns> Returns the position, width and height of the color cell. </returns>
        public static Rect GetCellRect(PaletteModifier pMod, Vector2Int point, out bool isPointInsideCG, out bool isTexPattern)
        {
            isPointInsideCG = false;
            isTexPattern = false;
            Rect cellRect = new Rect(0, 0, 0, 0);
            int x, y, width, height;

            for (int i = 0; i < pMod.texGrid.gridsList.Count; i++)
            {
                x = pMod.texGrid.gridsList[i].gridPos.x;
                y = pMod.texGrid.gridsList[i].gridPos.y;

                width = pMod.texGrid.gridsList[i].gridWidth;
                height = pMod.texGrid.gridsList[i].gridHeight;

                if (PointInsideRect(new Rect(x, y, width - 1, height - 1), point))
                {
                    if (pMod.texGrid.gridsList[i].isTexPattern)
                    {
                        cellRect = new Rect(x, y, width, height);
                        isPointInsideCG = true;
                        isTexPattern = true;        
                        break;
                    }

                    int len = pMod.texGrid.gridsList[i].vLinesOnTexGrid.Count;
                    int min, max;

                    for (int j = 0; j < len - 1; j++)
                    {
                        min = x + pMod.texGrid.gridsList[i].vLinesOnTexGrid[j];
                        max = x + pMod.texGrid.gridsList[i].vLinesOnTexGrid[j + 1];

                        if (point.x >= min && point.x <= max)
                        {
                            cellRect.x = min;
                            cellRect.width = max - min;
                            break;
                        }
                    }

                    len = pMod.texGrid.gridsList[i].hLinesOnTexGrid.Count;

                    for (int j = 0; j < len - 1; j++)
                    {
                        min = y + pMod.texGrid.gridsList[i].hLinesOnTexGrid[j];
                        max = y + pMod.texGrid.gridsList[i].hLinesOnTexGrid[j + 1];

                        if (point.y >= min && point.y <= max)
                        {
                            cellRect.y = min;
                            cellRect.height = max - min;
                            break;
                        }
                    }

                    isPointInsideCG = true;          
                }

                if (isPointInsideCG)
                    break;
            }

            return cellRect;
        }

        /// <summary>
        /// Checks whether a point is inside a rectangle.
        /// </summary>
        /// <param name="rect"> A rectangle. </param>
        /// <param name="point"> A 2D point. </param>
        /// <returns> Returns true if the point is inside the rectangle, otherwise returns false. </returns>
        public static bool PointInsideRect(Rect rect, Vector2Int point)
        {
            if (point.x < rect.x)
                return false;
            if (point.x > rect.x + rect.width)
                return false;

            if (point.y < rect.y)
                return false;
            if (point.y > rect.y + rect.height)
                return false;

            return true;
        }

        /// <summary>
        /// Updates the inspector colors based on the current texture colors.
        /// </summary>
        /// <param name="pMod"> Reference to a PaletteModifier script. </param>
        /// <param name="cellData"> Reference to CellData item. </param>
        /// <param name="tex"> Reference to a texture. </param>
        public static CellData GetCellColorFromTexture(PaletteModifier pMod, CellData cellData, Texture2D tex)
        {
            int x, y;
            Color texelColor;

            x = (int)(cellData.gridCell.x + cellData.gridCell.width * 0.5f);
            y = (int)(cellData.gridCell.y + cellData.gridCell.height * 0.5f);

            texelColor = tex.GetPixel(x, y);

            cellData.currentCellColor = texelColor;
            cellData.previousCellColor = texelColor;

            return cellData;
        }

        /// <summary>
        /// Checks whether there are unused grid cells on a texture. 
        /// </summary>
        /// <param name="pMod"> Reference to a PaletteModifier script. </param>
        /// <param name="tex"> Reference to a texture. </param>
        /// <param name="hasEmptyCells"> Used to determine if the texture has enough space for all the model colors. </param>
        /// <returns> Returns a list that contains the position, width, and height of the unused cells. </returns>
        public static List<Rect> GetEmptyGridCells(PaletteModifier pMod, Texture2D tex, out bool hasEmptyCells)
        {
            int cols, rows, x, y;
            int texelX, texelY;
            int emptyCellsCount = 0;
            int minX, maxX, minY, maxY;
            Color texelColor;
            List<Rect> emptyCells = new List<Rect>();

            hasEmptyCells = false;

            for (int i = 0; i < pMod.texGrid.gridsList.Count; i++)
            {
                if (pMod.texGrid.gridsList[i].hasEmptySpace)
                {
                    x = pMod.texGrid.gridsList[i].gridPos.x;
                    y = pMod.texGrid.gridsList[i].gridPos.y;

                    cols = pMod.texGrid.gridsList[i].gridColumns;
                    rows = pMod.texGrid.gridsList[i].gridRows;

                    for (int r = 0; r < rows; r++)
                    {
                        for (int c = 0; c < cols; c++)
                        {
                            minX = x + pMod.texGrid.gridsList[i].vLinesOnTexGrid[c];
                            maxX = x + pMod.texGrid.gridsList[i].vLinesOnTexGrid[c + 1];

                            minY = y + pMod.texGrid.gridsList[i].hLinesOnTexGrid[r];
                            maxY = y + pMod.texGrid.gridsList[i].hLinesOnTexGrid[r + 1];

                            texelX = Mathf.CeilToInt(minX + (maxX - minX) * 0.5f);
                            texelY = Mathf.CeilToInt(minY + (maxY - minY) * 0.5f);

                            texelColor = tex.GetPixel(texelX, texelY);

                            if (texelColor == pMod.texGrid.gridsList[i].emptySpaceColor)
                            {
                                emptyCells.Add(new Rect(minX, minY, maxX - minX, maxY - minY));
                                emptyCellsCount++;
                            }

                            if (emptyCellsCount == pMod.flatColorsOnObject)
                            {
                                hasEmptyCells = true;
                                return emptyCells;
                            }
                        }
                    }
                }
            }

            return emptyCells;
        }

        /// <summary>
        /// Compares two colors.
        /// </summary>
        /// <param name="colorOne"> First color. </param>
        /// <param name="colorTwo"> Second color. </param>
        /// <returns> Returns true if the colors are equal, otherwise returns false. </returns>
        public static bool AreColorsEqual(Color32 colorOne, Color32 colorTwo)
        {
            if (colorOne.r != colorTwo.r)
                return false;
            else if (colorOne.g != colorTwo.g)
                return false;
            else if (colorOne.b != colorTwo.b)
                return false;
            else if (colorOne.a != colorTwo.a)
                return false;
            else
                return true;
        }

        public static bool SetTextureGridReference(PaletteModifier pMod, Texture2D tex)
        {
            List<TextureGrid> tgList = FindAssetsByType<TextureGrid>();
            bool tgFound = false;

            for (int i = 0; i < tgList.Count; i++)
            {
                if (tex == tgList[i].texAtlas)
                {
                    pMod.texGrid = (TextureGrid)AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(tgList[i]), typeof(TextureGrid));
                    tgFound = true;
                    break;
                }
            }

            return tgFound;
        }

        public static List<T> FindAssetsByType<T>() where T : Object
        {
            List<T> assets = new List<T>();
            string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T)));

            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);

                if (asset != null)
                    assets.Add(asset);            
            }

            return assets;
        }
#endif
    }
}
