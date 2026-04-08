using System;
using System.Drawing;
using System.Windows.Forms;

namespace GpgPatcher.Gui
{
    internal sealed class MetricCard : ModernSurfacePanel
    {
        private readonly Label titleLabel;
        private readonly Label valueLabel;
        private readonly Label detailLabel;

        public MetricCard()
        {
            CornerRadius = 18;
            Padding = new Padding(14);
            Height = 108;
            Dock = DockStyle.Fill;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Color.Transparent,
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            Controls.Add(layout);

            titleLabel = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
            };
            valueLabel = new Label
            {
                AutoEllipsis = true,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
            };
            detailLabel = new Label
            {
                AutoEllipsis = true,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopLeft,
            };

            layout.Controls.Add(titleLabel, 0, 0);
            layout.Controls.Add(valueLabel, 0, 1);
            layout.Controls.Add(detailLabel, 0, 2);
        }

        public string TitleText
        {
            get { return titleLabel.Text; }
            set { titleLabel.Text = value; }
        }

        public string ValueText
        {
            get { return valueLabel.Text; }
            set { valueLabel.Text = value; }
        }

        public string DetailText
        {
            get { return detailLabel.Text; }
            set { detailLabel.Text = value; }
        }

        public void ApplyTheme(ThemePalette palette, bool highlighted)
        {
            FillColor = highlighted ? palette.SurfaceTint : palette.Surface;
            BorderColor = highlighted ? palette.BorderStrong : palette.Border;
            titleLabel.ForeColor = palette.TextSecondary;
            valueLabel.ForeColor = palette.TextPrimary;
            detailLabel.ForeColor = palette.TextSecondary;
            titleLabel.Font = palette.CreateUiFont(8.75f, FontStyle.Regular);
            valueLabel.Font = palette.CreateUiFont(14.5f, FontStyle.Bold);
            detailLabel.Font = palette.CreateUiFont(8.5f, FontStyle.Regular);
        }
    }
}
