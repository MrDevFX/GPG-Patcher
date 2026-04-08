using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace GpgPatcher.Gui
{
    internal static class BrandingAssets
    {
        private const string LogoResourceName = "GpgPatcher.Gui.Branding.logo.png";
        private const string IconResourceName = "GpgPatcher.Gui.Branding.app.ico";

        public static Image TryLoadLogoImage()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(LogoResourceName))
            {
                if (stream == null)
                {
                    return null;
                }

                using (var image = Image.FromStream(stream))
                {
                    return (Image)image.Clone();
                }
            }
        }

        public static Icon TryLoadApplicationIcon()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(IconResourceName))
            {
                if (stream != null)
                {
                    using (var icon = new Icon(stream))
                    {
                        return (Icon)icon.Clone();
                    }
                }
            }

            try
            {
                var executablePath = Application.ExecutablePath;
                if (string.IsNullOrWhiteSpace(executablePath) || !File.Exists(executablePath))
                {
                    return null;
                }

                using (var icon = Icon.ExtractAssociatedIcon(executablePath))
                {
                    return icon == null ? null : (Icon)icon.Clone();
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
