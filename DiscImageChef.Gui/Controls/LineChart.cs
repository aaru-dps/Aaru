using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Eto.Drawing;
using Eto.Forms;

namespace DiscImageChef.Gui.Controls
{
    /// <summary>
    ///     Draws a line chart
    /// </summary>
    public class LineChart : Drawable
    {
        bool       absoluteMargins;
        Color      axesColor;
        Color      backgroundColor;
        Color      colorX;
        Color      colorY;
        bool       drawAxes;
        Color      lineColor;
        float      marginX;
        float      marginXrated;
        float      marginY;
        float      marginYrated;
        float      maxX;
        float      maxY;
        float      minX;
        float      minY;
        PointF     previousPoint;
        float      ratioX;
        float      ratioY;
        RectangleF rect;
        bool       showStepsX;
        bool       showStepsY;
        float      stepsX;
        float      stepsY;

        public LineChart()
        {
            Values                   =  new ObservableCollection<PointF>();
            Values.CollectionChanged += OnValuesChanged;
            showStepsX               =  true;
            stepsX                   =  5;
            showStepsY               =  true;
            stepsY                   =  5;
            minX                     =  0;
            maxX                     =  100;
            minY                     =  0;
            maxY                     =  100;
            backgroundColor          =  Colors.Transparent;
            colorX                   =  Colors.DarkGray;
            colorY                   =  Colors.DarkGray;
            lineColor                =  Colors.Red;
            axesColor                =  Colors.Black;
            drawAxes                 =  true;
            marginX                  =  5;
            marginY                  =  5;
            absoluteMargins          =  true;
            previousPoint            =  new PointF(0, 0);
        }

        /// <summary>
        ///     If set the margins would be in absolute pixels, otherwise in relative points
        /// </summary>
        public bool AbsoluteMargins
        {
            get => absoluteMargins;
            set
            {
                if(absoluteMargins == value) return;

                absoluteMargins = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Margin between the leftmost border and the Y axis
        /// </summary>
        public float MarginX
        {
            get => marginX;
            set
            {
                marginX = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Margin between the bottommost border and the X axis
        /// </summary>
        public float MarginY
        {
            get => marginY;
            set
            {
                marginY = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Contains the relative poitns to be drawn
        /// </summary>
        public ObservableCollection<PointF> Values { get; }

        /// <summary>
        ///     If axes borders should be drawn
        /// </summary>
        public bool DrawAxes
        {
            get => drawAxes;
            set
            {
                if(drawAxes == value) return;

                drawAxes = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     If a grid should be drawn every X step
        /// </summary>
        public bool ShowStepsX
        {
            get => showStepsX;
            set
            {
                if(showStepsX == value) return;

                showStepsX = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Separation between X grid lines
        /// </summary>
        public float StepsX
        {
            get => stepsX;
            set
            {
                stepsX = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     If a grid should be drawn every Y step
        /// </summary>
        public bool ShowStepsY
        {
            get => showStepsY;
            set
            {
                if(showStepsY == value) return;

                showStepsY = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Separation between X grid lines
        /// </summary>
        public float StepsY
        {
            get => stepsY;
            set
            {
                stepsY = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Relative point that is equal to start of X
        /// </summary>
        public float MinX
        {
            get => minX;
            set
            {
                minX = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Relative point that is equal to start of Y
        /// </summary>
        public float MinY
        {
            get => minY;
            set
            {
                minY = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Relative point that is equal to end of X
        /// </summary>
        public float MaxX
        {
            get => maxX;
            set
            {
                maxX = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Relative point that is equal to end of Y
        /// </summary>
        public float MaxY
        {
            get => maxY;
            set
            {
                maxY = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Color for background
        /// </summary>
        public new Color BackgroundColor
        {
            get => backgroundColor;
            set
            {
                if(backgroundColor == value) return;

                backgroundColor = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Color to draw the axes borders
        /// </summary>
        public Color AxesColor
        {
            get => axesColor;
            set
            {
                if(axesColor == value) return;

                axesColor = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Color to draw the X grid
        /// </summary>
        public Color ColorX
        {
            get => colorX;
            set
            {
                if(colorX == value) return;

                colorX = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Color to draw the Y grid
        /// </summary>
        public Color ColorY
        {
            get => colorY;
            set
            {
                if(colorY == value) return;

                colorY = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Color to draw the line between points
        /// </summary>
        public Color LineColor
        {
            get => lineColor;
            set
            {
                if(lineColor == value) return;

                lineColor = value;
                Invalidate();
            }
        }

        void OnValuesChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            // If we do not support to drawn on the graphics we will need to redraw it, slowly
            if(!SupportsCreateGraphics) Invalidate();

            Graphics g;

            // If the control is not visible (hidden in another tab) this raises an exception
            try { g = CreateGraphics(); }
            catch { return; }

            switch(args.Action)
            {
                // Draw only next point
                case NotifyCollectionChangedAction.Add:
                    foreach(object item in args.NewItems)
                    {
                        if(!(item is PointF nextPoint)) continue;

                        float prevXrated = previousPoint.X * ratioX;
                        float prevYrated = previousPoint.Y * ratioY;

                        float nextXrated = nextPoint.X * ratioX;
                        float nextYrated = nextPoint.Y * ratioY;

                        g.DrawLine(lineColor, marginXrated + prevXrated, rect.Height - marginYrated - prevYrated,
                                   marginXrated            + nextXrated, rect.Height - marginYrated - nextYrated);

                        previousPoint = nextPoint;
                    }

                    break;
                // Need to redraw all points
                default:
                    Invalidate();
                    break;
            }

            g.Dispose();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            rect = e.ClipRectangle;

            g.FillRectangle(backgroundColor, rect);

            ratioX = rect.Width  / (maxX - minX);
            ratioY = rect.Height / (maxY - minY);

            marginXrated = marginX * (absoluteMargins ? 1 : ratioX);
            marginYrated = marginY * (absoluteMargins ? 1 : ratioY);

            if(drawAxes)
            {
                g.DrawLine(axesColor, marginXrated, 0, marginXrated, rect.Height);
                g.DrawLine(axesColor, 0, rect.Height - marginYrated, rect.Width,
                           rect.Height               - marginYrated);
            }

            if(showStepsX)
            {
                float stepsXraged = stepsX * ratioX;
                for(float x = marginXrated + stepsXraged; x < rect.Width;
                    x += stepsXraged)
                    g.DrawLine(colorX, x, 0, x, rect.Height - marginYrated - 1);
            }

            if(showStepsY)
            {
                float stepsYraged = stepsY * ratioY;
                for(float y = rect.Height - marginYrated - stepsYraged; y > 0; y -= stepsYraged)
                    g.DrawLine(colorY, marginXrated      + 1, y, rect.Width, y);
            }

            previousPoint = new PointF(0, 0);
            foreach(Point nextPoint in Values)
            {
                float prevXrated = previousPoint.X * ratioX;
                float prevYrated = previousPoint.Y * ratioY;

                float nextXrated = nextPoint.X * ratioX;
                float nextYrated = nextPoint.Y * ratioY;

                g.DrawLine(lineColor, marginXrated + prevXrated, rect.Height - marginYrated - prevYrated,
                           marginXrated            + nextXrated, rect.Height - marginYrated - nextYrated);

                previousPoint = nextPoint;
            }
        }
    }
}