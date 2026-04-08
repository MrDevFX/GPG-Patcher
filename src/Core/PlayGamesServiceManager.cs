using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.ServiceProcess;

namespace GpgPatcher
{
    internal static class PlayGamesServiceManager
    {
        private static readonly TimeSpan RestartTimeout = TimeSpan.FromSeconds(30);

        public static void Restart(PlayGamesInstallLayout layout)
        {
            Stop(layout);
            Start(layout);
        }

        public static void Stop(PlayGamesInstallLayout layout)
        {
            var serviceName = TryFindServiceName(layout.ServiceExePath);
            if (!string.IsNullOrEmpty(serviceName))
            {
                using (var controller = new ServiceController(serviceName))
                {
                    if (controller.Status == ServiceControllerStatus.Stopped)
                    {
                        return;
                    }

                    if (controller.Status != ServiceControllerStatus.StopPending)
                    {
                        controller.Stop();
                    }

                    controller.WaitForStatus(ServiceControllerStatus.Stopped, RestartTimeout);
                    return;
                }
            }

            foreach (var process in GetServiceProcesses(layout.ServiceExePath))
            {
                process.Kill();
                process.WaitForExit((int)RestartTimeout.TotalMilliseconds);
                process.Dispose();
            }
        }

        public static void Start(PlayGamesInstallLayout layout)
        {
            var serviceName = TryFindServiceName(layout.ServiceExePath);
            if (!string.IsNullOrEmpty(serviceName))
            {
                using (var controller = new ServiceController(serviceName))
                {
                    if (controller.Status != ServiceControllerStatus.Running
                        && controller.Status != ServiceControllerStatus.StartPending)
                    {
                        controller.Start();
                    }

                    controller.WaitForStatus(ServiceControllerStatus.Running, RestartTimeout);
                    return;
                }
            }

            using (var process = new Process())
            {
                process.StartInfo.FileName = layout.ServiceExePath;
                process.StartInfo.WorkingDirectory = layout.ServiceDirectory;
                process.StartInfo.UseShellExecute = false;
                process.Start();
            }
        }

        private static IEnumerable<Process> GetServiceProcesses(string serviceExePath)
        {
            foreach (var process in Process.GetProcessesByName("Service"))
            {
                string path = null;
                try
                {
                    path = process.MainModule == null ? null : process.MainModule.FileName;
                }
                catch
                {
                }

                if (string.Equals(path, serviceExePath, StringComparison.OrdinalIgnoreCase))
                {
                    yield return process;
                }
                else
                {
                    process.Dispose();
                }
            }
        }

        private static string TryFindServiceName(string serviceExePath)
        {
            var normalizedTarget = Path.GetFullPath(serviceExePath);
            using (var searcher = new ManagementObjectSearcher("SELECT Name, PathName FROM Win32_Service"))
            using (var services = searcher.Get())
            {
                foreach (ManagementObject service in services)
                {
                    var rawPath = service["PathName"] as string;
                    if (string.IsNullOrWhiteSpace(rawPath))
                    {
                        continue;
                    }

                    var executablePath = NormalizeServicePath(rawPath);
                    if (string.Equals(executablePath, normalizedTarget, StringComparison.OrdinalIgnoreCase))
                    {
                        return service["Name"] as string;
                    }
                }
            }

            return null;
        }

        private static string NormalizeServicePath(string rawPath)
        {
            rawPath = rawPath.Trim();
            if (rawPath.Length == 0)
            {
                return rawPath;
            }

            if (rawPath[0] == '"')
            {
                var endQuote = rawPath.IndexOf('"', 1);
                if (endQuote > 1)
                {
                    return Path.GetFullPath(rawPath.Substring(1, endQuote - 1));
                }
            }

            var exeIndex = rawPath.IndexOf(".exe", StringComparison.OrdinalIgnoreCase);
            if (exeIndex >= 0)
            {
                return Path.GetFullPath(rawPath.Substring(0, exeIndex + 4));
            }

            return Path.GetFullPath(rawPath);
        }
    }
}
