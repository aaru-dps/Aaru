// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ColoredGrid.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI custom controls.
//
// --[ Description ] ----------------------------------------------------------
//
//     Draws a colored grid.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General public License for more details.
//
//     You should have received a copy of the GNU General public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System.Collections.ObjectModel;
using Eto.Drawing;
using Eto.Forms;

namespace Aaru.Gui.Controls
{
    /// <summary>Draws a grid of colored blocks</summary>
    public class ColoredGrid : Drawable
    {
        /// <summary>Size of the block, including its top and left border, in pixels</summary>
        const int BLOCK_SIZE = 5;

        Color gridColor;

        public ColoredGrid()
        {
            ColoredBlocks                   =  new ObservableCollection<ColoredBlock>();
            ColoredBlocks.CollectionChanged += (sender, args) => Invalidate();
            gridColor                       =  Colors.Black;
        }

        new bool CanFocus => false;
        /// <summary>How many columns are in the grid</summary>
        public int Columns { get; private set; }
        /// <summary>How many rows are in the grid</summary>
        public int Rows { get; private set; }
        /// <summary>How many blocks are in the grid</summary>
        public ulong Blocks { get; private set; }

        public Color GridColor
        {
            get => gridColor;
            set
            {
                if(gridColor == value)
                    return;

                gridColor = value;
                Invalidate();
            }
        }

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
                graphics.DrawLine(gridColor, i, rect.Y, i, height);

            for(float i = rect.Y; i <= height; i += 5)
                graphics.DrawLine(gridColor, rect.X, i, width, i);

            Columns = width  / BLOCK_SIZE;
            Rows    = height / BLOCK_SIZE;
            Blocks  = (ulong)(Columns * Rows);

            foreach(ColoredBlock coloredBlock in ColoredBlocks)
                PaintBlock(graphics, coloredBlock.Color, coloredBlock.Block);
        }

        void PaintBlock(Graphics graphics, Color color, ulong block)
        {
            if(block > Blocks)
                return;

            int row = (int)(block / (ulong)Columns);
            int col = (int)(block % (ulong)Columns);
            int x   = col * BLOCK_SIZE;
            int y   = row * BLOCK_SIZE;

            graphics.FillRectangle(color, x + 1, y + 1, BLOCK_SIZE - 1, BLOCK_SIZE - 1);
        }
    }

    /// <summary>Defines a block that has a corresponding color</summary>
    public class ColoredBlock
    {
        public readonly ulong Block;
        public readonly Color Color;

        public ColoredBlock(ulong block, Color color)
        {
            Block = block;
            Color = color;
        }
    }
}