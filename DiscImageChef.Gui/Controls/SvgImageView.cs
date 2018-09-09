using System;
using System.IO;
using Eto.Drawing;
using Eto.Forms;

namespace DiscImageChef.Gui.Controls
{
    public class SvgImageView : ImageView
    {
        Stream svgStream;

        public new Image Image => base.Image;

        byte[] cachedRender;

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
            if(Width == -1 || Height == -1 || svgStream== null) return;

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