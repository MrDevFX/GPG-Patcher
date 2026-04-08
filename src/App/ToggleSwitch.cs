using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace GpgPatcher.Gui
{
    internal sealed class ToggleSwitch : CheckBox
    {
        public ToggleSwitch()
        {
            AutoSize = false;
            Size = new Size(52, 30);
            Cursor = Cursors.Hand;

            SetStyle(
                ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw
                | ControlStyles.UserPaint,
                true);
        }

        public ThemePalette Palette { get; set; }

        protected override void OnPaint(PaintEventArgs e)
        {
            var palette = Palette;
            if (palette == null)
            {
                base.OnPaint(e);
                return;
            }

            e.Graphics.Clear(Parent == null ? palette.WindowBackground : Parent.BackColor);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var trackBounds = new Rectangle(0, 2, 50, 26);
            var knobBounds = Checked
                ? new Rectangle(trackBounds.Right - 24, 5, 20, 20)
                : new Rectangle(trackBounds.Left + 4, 5, 20, 20);

            var trackColor = Enabled
                ? (Checked ? palette.Accent : palette.ProgressTrack)
                : palette.Border;

            var borderColor = Checked ? palette.Accent : palette.BorderStrong;
            var knobColor = Enabled ? Color.White : palette.SurfaceRaised;

            using (var trackPath = RoundedDrawing.CreateRoundedRectangle(trackBounds, 13))
            using (var trackBrush = new SolidBrush(trackColor))
            using (var borderPen = new Pen(borderColor))
            using (var knobBrush = new SolidBrush(knobColor))
            {
                e.Graphics.FillPath(trackBrush, trackPath);
                e.Graphics.DrawPath(borderPen, trackPath);
                e.Graphics.FillEllipse(knobBrush, knobBounds);
            }

            if (Focused)
            {
                ControlPaint.DrawFocusRectangle(e.Graphics, new Rectangle(0, 0, Width - 1, Height - 1));
            }
        }
    }
}
