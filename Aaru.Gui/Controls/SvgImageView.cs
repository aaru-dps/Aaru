// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : SvgImageView.xeto.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : SVG image view.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements a SVG rendering that can be used in the place of an Eto.ImageView.
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

using System;
using System.IO;
using Eto.Drawing;
using Eto.Forms;

namespace DiscImageChef.Gui.Controls
{
    public class SvgImageView : ImageView
    {
        byte[] cachedRender;
        Stream svgStream;

        public new Image Image => base.Image;

        public Stream SvgStream
        {
            get => svgStream;
            set
            {
                if(svgStream == value) return;

                svgStream = value;
                Redraw();
            }
        }

        void Redraw()
        {
            if(Width == -1 || Height == -1 || svgStream == null) return;

            svgStream.Position = 0;

            // TODO: Upstream library not working property: https://github.com/mono/SkiaSharp.Extended/issues/51

            /*SKSvg                svg      = new SKSvg();
            SKEncodedImageFormat skFormat = SKEncodedImageFormat.Png;
            svg.Load(svgStream);
            //SKRect   svgSize   = svg.Picture.CullRect;
            float canvasMin = Math.Min(Width, Height);
            float svgMax = Math.Max(svg.Picture.CullRect.Width, svg.Picture.CullRect.Height);
            float scale = canvasMin / svgMax;
            var matrix = SKMatrix.MakeScale(scale, scale);
            SKBitmap bitmap    = new SKBitmap((int)Width, (int)Height);
            SKCanvas canvas    = new SKCanvas(bitmap);
            canvas.DrawPicture(svg.Picture, ref matrix);
            canvas.Flush();
            SKImage      image = SKImage.FromBitmap(bitmap);
            SKData       data  = image.Encode(skFormat, 100);
            MemoryStream outMs = new MemoryStream();
            data.SaveTo(outMs);
            cachedRender = outMs.ToArray();
            base.Image   = new Bitmap(cachedRender);
            */
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            Redraw();
        }
    }
}