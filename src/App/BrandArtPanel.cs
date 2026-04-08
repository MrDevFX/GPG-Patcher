using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace GpgPatcher.Gui
{
    internal sealed class BrandArtPanel : Control
    {
        private Image logoImage;
        private Color borderColor = Color.FromArgb(72, 134, 214);
        private Color gradientStartColor = Color.FromArgb(20, 97, 194);
        private Color gradientEndColor = Color.FromArgb(35, 188, 213);
        private Color glowColor = Color.FromArgb(110, 255, 255, 255);
        private Color orbColor = Color.FromArgb(72, 165, 243, 252);
        private Color textColor = Color.White;

        public BrandArtPanel()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw
                | ControlStyles.UserPaint,
                true);

            Size = new Size(178, 112);
            Margin = new Padding(0, 0, 18, 0);
        }

        public Image LogoImage
        {
            get { return logoImage; }
            set
            {
                logoImage = value;
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

        public Color GlowColor
        {
            get { return glowColor; }
            set
            {
                glowColor = value;
                Invalidate();
            }
        }

        public Color OrbColor
        {
            get { return orbColor; }
            set
            {
                orbColor = value;
                Invalidate();
            }
        }

        public Color TextColor
        {
            get { return textColor; }
            set
            {
                textColor = value;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            e.Graphics.Clear(Parent == null ? BackColor : Parent.BackColor);

            var bounds = new Rectangle(0, 0, Width - 1, Height - 1);
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            using (var path = RoundedDrawing.CreateRoundedRectangle(bounds, 24))
            using (var brush = new LinearGradientBrush(bounds, gradientStartColor, gradientEndColor, 20f))
            using (var borderPen = new Pen(borderColor))
            {
                e.Graphics.FillPath(brush, path);
                DrawBackgroundDecorations(e.Graphics, bounds);
                e.Graphics.DrawPath(borderPen, path);
            }

            DrawLogo(e.Graphics, bounds);
        }

        private void DrawBackgroundDecorations(Graphics graphics, Rectangle bounds)
        {
            var glowRect = new Rectangle(bounds.Left + 74, bounds.Top - 28, 120, 120);
            using (var glowBrush = new SolidBrush(glowColor))
            {
                graphics.FillEllipse(glowBrush, glowRect);
            }

            using (var orbBrush = new SolidBrush(orbColor))
            {
                graphics.FillEllipse(orbBrush, bounds.Left + 18, bounds.Top + 18, 22, 22);
                graphics.FillEllipse(orbBrush, bounds.Right - 34, bounds.Bottom - 32, 14, 14);
            }

            using (var accentPen = new Pen(Color.FromArgb(56, 255, 255, 255), 2f))
            {
                graphics.DrawLine(accentPen, bounds.Left + 18, bounds.Bottom - 22, bounds.Left + 58, bounds.Bottom - 10);
                graphics.DrawLine(accentPen, bounds.Right - 70, bounds.Top + 20, bounds.Right - 18, bounds.Top + 12);
            }
        }

        private void DrawLogo(Graphics graphics, Rectangle bounds)
        {
            var logoBounds = Rectangle.Inflate(bounds, -18, -10);
            if (logoImage != null)
            {
                var scaled = GetScaledBounds(logoImage.Size, logoBounds);
                graphics.DrawImage(logoImage, scaled);
                return;
            }

            using (var textBrush = new SolidBrush(textColor))
            using (var font = new Font("Segoe UI", 18f, FontStyle.Bold, GraphicsUnit.Point))
            {
                var format = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                };

                graphics.DrawString("GPG", font, textBrush, logoBounds, format);
            }
        }

        private static Rectangle GetScaledBounds(Size imageSize, Rectangle bounds)
        {
            if (imageSize.Width <= 0 || imageSize.Height <= 0)
            {
                return bounds;
            }

            var scale = Math.Min(bounds.Width / (float)imageSize.Width, bounds.Height / (float)imageSize.Height);
            var width = Math.Max(1, (int)Math.Round(imageSize.Width * scale));
            var height = Math.Max(1, (int)Math.Round(imageSize.Height * scale));
            var x = bounds.Left + ((bounds.Width - width) / 2);
            var y = bounds.Top + ((bounds.Height - height) / 2);
            return new Rectangle(x, y, width, height);
        }
    }
}
