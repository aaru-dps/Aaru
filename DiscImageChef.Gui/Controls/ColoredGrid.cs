using System.Collections.ObjectModel;
using Eto.Drawing;
using Eto.Forms;

namespace DiscImageChef.Gui.Controls
{
    /// <summary>
    ///     Draws a grid of colored blocks
    /// </summary>
    public class ColoredGrid : Drawable
    {
        /// <summary>
        ///     Size of the block, including its top and left border, in pixels
        /// </summary>
        const int BLOCK_SIZE = 5;

        public ColoredGrid()
        {
            ColoredBlocks                   =  new ObservableCollection<ColoredBlock>();
            ColoredBlocks.CollectionChanged += (sender, args) => Invalidate();
        }

        new bool CanFocus => false;
        /// <summary>
        ///     How many columns are in the grid
        /// </summary>
        public int Columns { get; private set; }
        /// <summary>
        ///     How many rows are in the grid
        /// </summary>
        public int Rows { get; private set; }
        /// <summary>
        ///     How many blocks are in the grid
        /// </summary>
        public int Blocks { get; private set; }

        public ObservableCollection<ColoredBlock> ColoredBlocks { get; }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics   graphics = e.Graphics;
            RectangleF rect     = e.ClipRectangle;

            int remainder = (int)rect.Width % (BLOCK_SIZE + 1);
            int width     = (int)rect.Width - remainder - 1;
            remainder = (int)rect.Height % (BLOCK_SIZE  + 1);
            int height = (int)rect.Height - remainder   - 1;

            for(float i = rect.X; i <= width; i += 5)
                graphics.DrawLine(Color.FromRgb(0x00000000), i, rect.Y, i, height);

            for(float i = rect.Y; i <= height; i += 5)
                graphics.DrawLine(Color.FromRgb(0x00000000), rect.X, i, width, i);

            Columns = width   / BLOCK_SIZE;
            Rows    = height  / BLOCK_SIZE;
            Blocks  = Columns * Rows;

            foreach(ColoredBlock coloredBlock in ColoredBlocks)
                PaintBlock(graphics, coloredBlock.Color, coloredBlock.Block);
        }

        void PaintBlock(Graphics graphics, Color color, int block)
        {
            if(block > Blocks) return;

            int row = block / Columns;
            int col = block % Columns;
            int x   = col   * BLOCK_SIZE;
            int y   = row   * BLOCK_SIZE;

            graphics.FillRectangle(color, x + 1, y + 1, BLOCK_SIZE - 1, BLOCK_SIZE - 1);
        }
    }

    /// <summary>
    ///     Defines a block that has a corresponding color
    /// </summary>
    public class ColoredBlock
    {
        public readonly int   Block;
        public readonly Color Color;

        public ColoredBlock(int block, Color color)
        {
            Block = block;
            Color = color;
        }
    }
}