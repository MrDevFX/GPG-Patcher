using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace GpgPatcher.Gui
{
    internal class ModernSurfacePanel : Panel
    {
        private int cornerRadius = 20;
        private Color fillColor = Color.White;
        private Color borderColor = Color.Gainsboro;
        private bool useGradient;
        private Color gradientStartColor = Color.White;
        private Color gradientEndColor = Color.White;

        public ModernSurfacePanel()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw
                | ControlStyles.UserPaint,
                true);
        }

        public int CornerRadius
        {
            get { return cornerRadius; }
            set
            {
                cornerRadius = value;
                Invalidate();
            }
        }

        public Color FillColor
        {
            get { return fillColor; }
            set
            {
                fillColor = value;
                Invalidate();
            }
        }

        public Color BorderColor
        {
            get { return borderColor; }
            set
            {
                borderColor = value;
                Invalidate();
            }
        }

        public bool UseGradient
        {
            get { return useGradient; }
            set
            {
                useGradient = value;
                Invalidate();
            }
        }

        public Color GradientStartColor
        {
            get { return gradientStartColor; }
            set
            {
                gradientStartColor = value;
                Invalidate();
            }
        }

        public Color GradientEndColor
        {
            get { return gradientEndColor; }
            set
            {
                gradientEndColor = value;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(Parent == null ? fillColor : Parent.BackColor);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var bounds = new Rectangle(0, 0, Width - 1, Height - 1);
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            using (var path = RoundedDrawing.CreateRoundedRectangle(bounds, cornerRadius))
            using (var borderPen = new Pen(borderColor))
            {
                if (useGradient)
                {
                    using (var brush = new LinearGradientBrush(bounds, gradientStartColor, gradientEndColor, LinearGradientMode.Horizontal))
                    {
                        e.Graphics.FillPath(brush, path);
                    }
                }
                else
                {
                    using (var brush = new SolidBrush(fillColor))
                    {
                        e.Graphics.FillPath(brush, path);
                    }
                }

                e.Graphics.DrawPath(borderPen, path);
            }
        }
    }
}
