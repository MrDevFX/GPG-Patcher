using System;
using System.Linq;
using System.Windows.Forms;
using GpgPatcher;

namespace GpgPatcher.Gui
{
    internal static class Program
    {
        [STAThread]
        private static int Main(string[] args)
        {
            if (args != null
                && args.Length > 0
                && string.Equals(args[0], "--headless", StringComparison.OrdinalIgnoreCase))
            {
                return PatcherCommandHost.Run(args.Skip(1).ToArray());
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
            return 0;
        }
    }
}
