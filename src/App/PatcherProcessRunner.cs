using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GpgPatcher.Gui
{
    internal sealed class PatcherProcessRunner
    {
        private readonly string hostPath;

        public PatcherProcessRunner()
        {
            using (var process = Process.GetCurrentProcess())
            {
                hostPath = process.MainModule == null
                    ? Application.ExecutablePath
                    : process.MainModule.FileName;
            }
        }

        public string HostPath
        {
            get { return hostPath; }
        }

        public bool HostExists
        {
            get { return File.Exists(hostPath); }
        }

        public Task<CommandResult> RunCapturedAsync(string arguments)
        {
            return Task.Run(() => RunCaptured(arguments));
        }

        public Task<int> RunElevatedAsync(string arguments)
        {
            return Task.Run(() => RunElevated(arguments));
        }

        private CommandResult RunCaptured(string arguments)
        {
            EnsureHostExists();

            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = hostPath,
                    Arguments = BuildHeadlessArguments(arguments),
                    WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                };

                process.Start();
                var stdout = process.StandardOutput.ReadToEnd();
                var stderr = process.StandardError.ReadToEnd();
                process.WaitForExit();

                return new CommandResult
                {
                    ExitCode = process.ExitCode,
                    StandardOutput = stdout,
                    StandardError = stderr,
                };
            }
        }

        private int RunElevated(string arguments)
        {
            EnsureHostExists();

            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = hostPath,
                    Arguments = BuildHeadlessArguments(arguments),
                    WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
                    UseShellExecute = true,
                    Verb = "runas",
                };

                process.Start();
                process.WaitForExit();
                return process.ExitCode;
            }
        }

        private void EnsureHostExists()
        {
            if (!HostExists)
            {
                throw new FileNotFoundException("Could not find the GPG Patcher executable required to run maintenance commands.", hostPath);
            }
        }

        private static string BuildHeadlessArguments(string arguments)
        {
            if (string.IsNullOrWhiteSpace(arguments))
            {
                return "--headless";
            }

            return "--headless " + arguments.Trim();
        }
    }
}
