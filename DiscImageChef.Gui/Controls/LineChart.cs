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
        bool  absoluteMargins;
        Color axesColor;
        Color backgroundColor;
        Color colorX;
        Color colorY;
        bool  drawAxes;
        Color lineColor;
        float marginX;
        float marginY;
        float maxX;
        float maxY;
        float minX;
        float minY;
        float ratioX;
        float ratioY;
        bool  showStepsX;
        bool  showStepsY;
        float stepsX;
        float stepsY;

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
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics   g = e.Graphics;
            RectangleF r = e.ClipRectangle;

            g.FillRectangle(backgroundColor, r);

            ratioX = r.Width  / (maxX - minX);
            ratioY = r.Height / (maxY - minY);

            float marginXrated = marginX * (absoluteMargins ? 1 : ratioX);
            float marginYrated = marginY * (absoluteMargins ? 1 : ratioY);

            if(drawAxes)
            {
                g.DrawLine(axesColor, marginXrated, 0,                       marginXrated, r.Height);
                g.DrawLine(axesColor, 0,            r.Height - marginYrated, r.Width,      r.Height - marginYrated);
            }

            if(showStepsX)
            {
                float stepsXraged = stepsX * ratioX;
                for(float x = marginXrated                              + stepsXraged; x < r.Width; x += stepsXraged)
                    g.DrawLine(colorX, x, 0, x, r.Height - marginYrated - 1);
            }

            if(showStepsY)
            {
                float stepsYraged = stepsY * ratioY;
                for(float y = r.Height - marginYrated - stepsYraged; y > 0; y -= stepsYraged)
                    g.DrawLine(colorY, marginXrated   + 1, y, r.Width, y);
            }

            PointF previousPoint = new PointF(0, 0);
            foreach(Point nextPoint in Values)
            {
                float prevXrated = previousPoint.X * ratioX;
                float prevYrated = previousPoint.Y * ratioY;

                float nextXrated = nextPoint.X * ratioX;
                float nextYrated = nextPoint.Y * ratioY;

                g.DrawLine(lineColor, marginXrated + prevXrated, r.Height - marginYrated - prevYrated,
                           marginXrated            + nextXrated, r.Height - marginYrated - nextYrated);

                previousPoint = nextPoint;
            }
        }
    }
}