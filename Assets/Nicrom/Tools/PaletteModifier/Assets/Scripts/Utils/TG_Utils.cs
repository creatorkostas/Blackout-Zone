using UnityEngine;

namespace Nicrom.PM {
    public static class TG_Utils {

        /// <summary>
        /// Makes sure a grid is always inside the texture bounds.
        /// </summary>
        /// <param name="pos"> Position of a grid. </param>
        /// <param name="size"> Size of a grid. </param>
        public static Vector2Int ClampRectPosToTexture(TextureGrid tg, Vector2Int pos, Vector2 size)
        {
            if (pos.x < 0)
                pos.x = 0;

            if (pos.y < 0)
                pos.y = 0;

            if (pos.x + size.x > tg.texAtlas.width)
                pos.x = (int)(tg.texAtlas.width - size.x);

            if (pos.y + size.y > tg.texAtlas.height)
                pos.y = (int)(tg.texAtlas.height - size.y);

            return pos;
        }

        /// <summary>
        /// Clamps the width of a rectangle.
        /// </summary>
        /// <param name="tg"> Reference to a TextureGrid. </param>
        /// <param name="cg"> Reference to a CustomGrid. </param>
        /// <returns> Returns the clamped width. </returns>
        public static int ClampRectWidth(TextureGrid tg, CustomGrid cg)
        {
            int rectWidth = cg.gridWidth;

            if ((cg.gridPos.x + rectWidth) > tg.texAtlas.width)
                rectWidth = tg.texAtlas.width - cg.gridPos.x;

            if ((float)rectWidth / cg.gridColumns < 3)
                rectWidth = cg.gridColumns * 3;

            if (rectWidth > 4096)
                rectWidth = 4096;

            return rectWidth;
        }

        /// <summary>
        /// Clamps the height of a rectangle.
        /// </summary>
        /// <param name="tg"> Reference to a TextureGrid. </param>
        /// <param name="cg"> Reference to a CustomGrid. </param>
        /// <returns> Returns the clamped height. </returns>
        public static int ClampRectHeight(TextureGrid tg, CustomGrid cg)
        {
            int rectHeight = cg.gridHeight;

            if ((cg.gridPos.y + rectHeight) > tg.texAtlas.height)
                rectHeight = tg.texAtlas.height - cg.gridPos.y;

            if ((float)rectHeight / cg.gridRows < 3)
                rectHeight = cg.gridRows * 3;

            if (rectHeight > 4096)
                rectHeight = 4096;

            return rectHeight;
        }

        /// <summary>
        /// Clamps the position of a grid or handle.
        /// </summary>
        /// <param name="pos"> Position in texture pixel space. </param>
        /// <param name="size"> Size of the grid or handle. </param>
        /// <returns> Returns the clamped position. </returns>
        public static Vector2 ClampPosToCanvas(TextureGrid tg, Vector2 pos, Vector2 size)
        {
            if (pos.x < 0)
                pos.x = 0;

            if (pos.y < 0)
                pos.y = 0;

            if (pos.x + size.x > (tg.texAtlas.width + tg.sizeOffset.x))
                pos.x = (tg.texAtlas.width + tg.sizeOffset.x) - size.x;

            if (pos.y + size.y > (tg.texAtlas.height + tg.sizeOffset.y))
                pos.y = (tg.texAtlas.height + tg.sizeOffset.y) - size.y;

            return pos;
        }

        /// <summary>
        /// Clamps the number of columns a grid has.
        /// </summary>
        /// <param name="tg"> Reference to a TextureGrid. </param>
        /// <param name="cg"> Reference to a CustomGrid. </param>
        /// <returns> Returns the clamped columns number. </returns>
        public static int ClampGridColumns(CustomGrid cg)
        {
            int col = cg.gridColumns;

            if (col < 1)
                col = 1;

            float width = (float)cg.gridWidth / col;

            if (width < 3)
                col = cg.gridWidth / 3;

            return col;
        }

        /// <summary>
        /// Clamps the number of rows a grid has.
        /// </summary>
        /// <param name="tg"> Reference to a TextureGrid. </param>
        /// <param name="cg"> Reference to a CustomGrid. </param>
        /// <returns> Returns the clamped rows number. </returns>
        public static int ClampGridRows(CustomGrid cg)
        {
            int rows = cg.gridRows;

            if (rows < 1)
                rows = 1;

            float width = (float)cg.gridHeight / rows;

            if (width < 3)
                rows = cg.gridHeight / 3;

            return rows;
        }

        /// <summary>
        /// Checks whether 2 grids overlap.
        /// </summary>
        /// <param name="cg1"> Reference to the first CustomGrid. </param>
        /// <param name="cg2"> Reference to the second CustomGrid. </param>
        /// <returns> Returns true if the grids overlap, otherwise returns false. </returns>
        public static bool AreGridsOverlapping(CustomGrid cg1, CustomGrid cg2)
        {
            if (cg1.gridPos.x + cg1.gridWidth - 1 < cg2.gridPos.x || cg1.gridPos.x > cg2.gridPos.x + cg2.gridWidth - 1)
                return false;

            if (cg1.gridPos.y + cg1.gridHeight - 1 < cg2.gridPos.y || cg1.gridPos.y > cg2.gridPos.y + cg2.gridHeight - 1)
                return false;

            return true;
        }
    }
}
