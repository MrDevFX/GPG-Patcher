using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace GpgPatcher.Gui
{
    internal sealed class StatusChip : Control
    {
        private int cornerRadius = 14;

        public StatusChip()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw
                | ControlStyles.UserPaint,
                true);

            AutoSize = false;
            Padding = new Padding(14, 6, 14, 6);
            Size = new Size(120, 30);
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

        public Color FillColor { get; set; }

        public Color BorderColor { get; set; }

        public Color TextColor { get; set; }

        public override string Text
        {
            get { return base.Text; }
            set
            {
                base.Text = value;
                UpdatePreferredWidth();
                Invalidate();
            }
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            UpdatePreferredWidth();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(Parent == null ? SystemColors.Control : Parent.BackColor);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var bounds = new Rectangle(0, 0, Width - 1, Height - 1);

            using (var path = RoundedDrawing.CreateRoundedRectangle(bounds, cornerRadius))
            using (var fillBrush = new SolidBrush(FillColor))
            using (var borderPen = new Pen(BorderColor))
            {
                e.Graphics.FillPath(fillBrush, path);
                e.Graphics.DrawPath(borderPen, path);
            }

            TextRenderer.DrawText(
                e.Graphics,
                Text,
                Font,
                bounds,
                TextColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        private void UpdatePreferredWidth()
        {
            var textSize = TextRenderer.MeasureText(Text ?? string.Empty, Font);
            Width = Math.Max(74, textSize.Width + Padding.Left + Padding.Right);
            Height = Math.Max(30, textSize.Height + Padding.Top + Padding.Bottom - 2);
        }
    }
}
