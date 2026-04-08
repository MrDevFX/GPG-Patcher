using System;
using System.Drawing;
using Microsoft.Win32;

namespace GpgPatcher.Gui
{
    internal sealed class ThemePalette
    {
        private ThemePalette()
        {
        }

        public bool IsDark { get; private set; }

        public Color WindowBackground { get; private set; }

        public Color Surface { get; private set; }

        public Color SurfaceRaised { get; private set; }

        public Color SurfaceTint { get; private set; }

        public Color Border { get; private set; }

        public Color BorderStrong { get; private set; }

        public Color TextPrimary { get; private set; }

        public Color TextSecondary { get; private set; }

        public Color Accent { get; private set; }

        public Color AccentHover { get; private set; }

        public Color AccentPressed { get; private set; }

        public Color AccentSoft { get; private set; }

        public Color Success { get; private set; }

        public Color SuccessSoft { get; private set; }

        public Color Danger { get; private set; }

        public Color DangerSoft { get; private set; }

        public Color Warning { get; private set; }

        public Color WarningSoft { get; private set; }

        public Color CodeBackground { get; private set; }

        public Color CodeText { get; private set; }

        public Color ProgressTrack { get; private set; }

        public static ThemePalette Detect()
        {
            var useLight = DetectWindowsLightTheme();
            return useLight ? CreateLight() : CreateDark();
        }

        public Font CreateUiFont(float size, FontStyle style)
        {
            try
            {
                return new Font("Segoe UI Variable Text", size, style, GraphicsUnit.Point);
            }
            catch
            {
                return new Font("Segoe UI", size, style, GraphicsUnit.Point);
            }
        }

        public Font CreateMonoFont(float size)
        {
            try
            {
                return new Font("Cascadia Code", size, FontStyle.Regular, GraphicsUnit.Point);
            }
            catch
            {
                return new Font("Consolas", size, FontStyle.Regular, GraphicsUnit.Point);
            }
        }

        private static bool DetectWindowsLightTheme()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    var value = key == null ? null : key.GetValue("AppsUseLightTheme");
                    if (value is int intValue)
                    {
                        return intValue != 0;
                    }
                }
            }
            catch
            {
            }

            return true;
        }

        private static ThemePalette CreateDark()
        {
            return new ThemePalette
            {
                IsDark = true,
                WindowBackground = Color.FromArgb(12, 18, 30),
                Surface = Color.FromArgb(18, 26, 43),
                SurfaceRaised = Color.FromArgb(22, 33, 53),
                SurfaceTint = Color.FromArgb(20, 39, 68),
                Border = Color.FromArgb(43, 58, 84),
                BorderStrong = Color.FromArgb(61, 83, 120),
                TextPrimary = Color.FromArgb(237, 242, 250),
                TextSecondary = Color.FromArgb(150, 163, 185),
                Accent = Color.FromArgb(88, 166, 255),
                AccentHover = Color.FromArgb(110, 181, 255),
                AccentPressed = Color.FromArgb(58, 142, 238),
                AccentSoft = Color.FromArgb(42, 70, 110),
                Success = Color.FromArgb(52, 211, 153),
                SuccessSoft = Color.FromArgb(22, 101, 82),
                Danger = Color.FromArgb(248, 113, 113),
                DangerSoft = Color.FromArgb(127, 29, 29),
                Warning = Color.FromArgb(251, 191, 36),
                WarningSoft = Color.FromArgb(120, 53, 15),
                CodeBackground = Color.FromArgb(9, 14, 24),
                CodeText = Color.FromArgb(224, 232, 245),
                ProgressTrack = Color.FromArgb(31, 41, 55),
            };
        }

        private static ThemePalette CreateLight()
        {
            return new ThemePalette
            {
                IsDark = false,
                WindowBackground = Color.FromArgb(243, 247, 252),
                Surface = Color.FromArgb(255, 255, 255),
                SurfaceRaised = Color.FromArgb(248, 251, 255),
                SurfaceTint = Color.FromArgb(239, 246, 255),
                Border = Color.FromArgb(213, 223, 236),
                BorderStrong = Color.FromArgb(173, 193, 221),
                TextPrimary = Color.FromArgb(15, 23, 42),
                TextSecondary = Color.FromArgb(71, 85, 105),
                Accent = Color.FromArgb(37, 99, 235),
                AccentHover = Color.FromArgb(29, 78, 216),
                AccentPressed = Color.FromArgb(30, 64, 175),
                AccentSoft = Color.FromArgb(191, 219, 254),
                Success = Color.FromArgb(22, 163, 74),
                SuccessSoft = Color.FromArgb(220, 252, 231),
                Danger = Color.FromArgb(220, 38, 38),
                DangerSoft = Color.FromArgb(254, 226, 226),
                Warning = Color.FromArgb(217, 119, 6),
                WarningSoft = Color.FromArgb(254, 243, 199),
                CodeBackground = Color.FromArgb(20, 28, 45),
                CodeText = Color.FromArgb(236, 241, 248),
                ProgressTrack = Color.FromArgb(226, 232, 240),
            };
        }
    }
}
