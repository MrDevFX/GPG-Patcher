using System;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Xml.Linq;

namespace GpgPatcher
{
    public static class PatcherCommandHost
    {
        public static int Run(string[] args)
        {
            try
            {
                return RunCore(args ?? Array.Empty<string>());
            }
            catch (FriendlyException ex)
            {
                Console.Error.WriteLine("error: " + ex.Message);
                return 1;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("error: " + ex.Message);
                return 1;
            }
        }

        private static int RunCore(string[] args)
        {
            if (args.Length == 0 || IsHelp(args[0]))
            {
                PrintUsage();
                return 0;
            }

            var layout = PlayGamesInstallLayout.CreateDefault();
            var command = args[0].Trim().ToLowerInvariant();
            var enablePhenotypeFallback = args.Skip(1)
                .Any(argument => string.Equals(argument, "--phenotype-fallback", StringComparison.OrdinalIgnoreCase));

            switch (command)
            {
                case "inspect":
                    Inspect(layout);
                    return 0;
                case "patch":
                    Patch(layout, enablePhenotypeFallback);
                    return 0;
                case "verify":
                    return Verify(layout);
                case "restore":
                    Restore(layout);
                    return 0;
                default:
                    throw new FriendlyException("Unknown command '" + args[0] + "'.");
            }
        }

        private static void Inspect(PlayGamesInstallLayout layout)
        {
            var version = PatchStatusInspector.GetInstalledVersion(layout);
            var patchStatus = PatchStatusInspector.Inspect(layout);
            var launch = LogParser.TryGetLatestLaunch(layout.ServiceLogPath, GpgConstants.TargetPackageName);
            var cap = LogParser.TryGetLatestResolutionCap(layout.ServiceLogPath);
            var androidSerial = LogParser.TryGetLatestAndroidSerialDisplay(layout.AndroidSerialLogPath);

            Console.WriteLine("Google Play Games");
            Console.WriteLine("  version: " + version);
            Console.WriteLine("  compatible: " + string.Equals(version, GpgConstants.SupportedVersion, StringComparison.Ordinal));
            Console.WriteLine("  service dir: " + layout.ServiceDirectory);
            Console.WriteLine();

            Console.WriteLine("Patch status");
            Console.WriteLine("  service lib patched: " + patchStatus.IsPatched);
            Console.WriteLine("  available-settings hook: " + patchStatus.AvailableSettingsPatched);
            Console.WriteLine("  launch-settings hook: " + patchStatus.LaunchSettingsPatched);
            Console.WriteLine("  hook dll present: " + patchStatus.HookDllPresent);
            Console.WriteLine("  backup present: " + patchStatus.BackupPresent);
            Console.WriteLine("  phenotype override present: " + patchStatus.PhenotypeOverridePresent);
            if (patchStatus.PhenotypeOverridePresent && !string.IsNullOrWhiteSpace(patchStatus.PhenotypeOverrideValue))
            {
                Console.WriteLine("  phenotype override value: " + patchStatus.PhenotypeOverrideValue.Trim());
            }

            Console.WriteLine();
            Console.WriteLine("Latest " + GpgConstants.TargetPackageName + " launch");
            if (launch == null)
            {
                Console.WriteLine("  launch settings: not found in " + layout.ServiceLogPath);
            }
            else
            {
                Console.WriteLine("  timestamp: " + FormatTimestamp(launch.Timestamp));
                Console.WriteLine("  density: " + (launch.DisplayDensity.HasValue ? launch.DisplayDensity.Value.ToString() : "(unknown)"));
                Console.WriteLine("  display size: " + (launch.DisplaySize == null ? "(unknown)" : launch.DisplaySize.ToString()));
                Console.WriteLine("  available sizes: " + LogParser.FormatDisplaySizes(launch.AvailableDisplaySizes));
                Console.WriteLine("  display id: " + (launch.DisplayId.HasValue ? launch.DisplayId.Value.ToString() : "(unknown)"));
            }

            Console.WriteLine();
            Console.WriteLine("Latest related caps and guest display");
            Console.WriteLine("  resolution cap: " + (cap == null ? "(not found)" : cap.Cap));
            Console.WriteLine("  android serial display: " + (androidSerial == null ? "(not found)" : androidSerial.DisplaySize.ToString()));
        }

        private static void Patch(PlayGamesInstallLayout layout, bool enablePhenotypeFallback)
        {
            EnsureAdministrator();
            layout.EnsureInstallationExists();
            layout.EnsureHookBuildExists();
            EnsureSupportedVersion(layout);

            var patchStatus = PatchStatusInspector.Inspect(layout);
            if (!patchStatus.BackupPresent && patchStatus.IsPatched)
            {
                throw new FriendlyException(
                    "The service already looks patched, but no pristine backup exists in '" + layout.BackupDirectory + "'. Restore would be unsafe, so patch is aborting.");
            }

            PlayGamesServiceManager.Stop(layout);
            var startedAgain = false;
            try
            {
                BackupOriginalFiles(layout);

                var tempPatchedServiceLib = Path.Combine(Path.GetTempPath(), "gpg-patcher-ServiceLib.dll");
                if (File.Exists(tempPatchedServiceLib))
                {
                    File.Delete(tempPatchedServiceLib);
                }

                var patchResult = ServiceLibPatcher.Patch(layout.ServiceLibPath, tempPatchedServiceLib);
                File.Copy(tempPatchedServiceLib, layout.ServiceLibPath, true);
                File.Delete(tempPatchedServiceLib);

                File.Copy(layout.HookSourcePath, layout.HookTargetPath, true);
                DeleteIfExists(layout.LegacyHookTargetPath);

                if (enablePhenotypeFallback)
                {
                    ApplyPhenotypeFallback(layout);
                }

                PlayGamesServiceManager.Start(layout);
                startedAgain = true;

                Console.WriteLine("Patch complete");
                Console.WriteLine("  available-settings method changed: " + patchResult.AvailableSettingsPatched);
                Console.WriteLine("  launch-settings method changed: " + patchResult.LaunchSettingsPatched);
                Console.WriteLine("  hook dll copied: " + layout.HookTargetPath);
                Console.WriteLine("  phenotype fallback applied: " + enablePhenotypeFallback);
                Console.WriteLine();
                Console.WriteLine("Next step");
                Console.WriteLine("  launch Whiteout Survival, then run Verify from the app.");
            }
            finally
            {
                if (!startedAgain)
                {
                    try
                    {
                        PlayGamesServiceManager.Start(layout);
                    }
                    catch
                    {
                    }
                }
            }
        }

        private static int Verify(PlayGamesInstallLayout layout)
        {
            var launch = LogParser.TryGetLatestLaunch(layout.ServiceLogPath, GpgConstants.TargetPackageName);
            var cap = LogParser.TryGetLatestResolutionCap(layout.ServiceLogPath);
            var androidSerial = LogParser.TryGetLatestAndroidSerialDisplay(layout.AndroidSerialLogPath);
            var patchStatus = PatchStatusInspector.Inspect(layout);

            var expectedDisplaySize = new DisplaySizeSnapshot(GpgConstants.TargetWidth, GpgConstants.TargetHeight);
            var expectedDensity = 359;

            var launchMatches = launch != null
                && launch.DisplaySize != null
                && launch.DisplaySize.Width == expectedDisplaySize.Width
                && launch.DisplaySize.Height == expectedDisplaySize.Height
                && launch.DisplayDensity == expectedDensity
                && launch.AvailableDisplaySizes.Any(size => size.Width == expectedDisplaySize.Width && size.Height == expectedDisplaySize.Height);

            var serialMatches = androidSerial != null
                && androidSerial.DisplaySize.Width == expectedDisplaySize.Width
                && androidSerial.DisplaySize.Height == expectedDisplaySize.Height;

            Console.WriteLine("Verify");
            Console.WriteLine("  expected launch size: " + expectedDisplaySize);
            Console.WriteLine("  expected scaled density: " + expectedDensity);
            Console.WriteLine("  latest launch size: " + (launch == null || launch.DisplaySize == null ? "(not found)" : launch.DisplaySize.ToString()));
            Console.WriteLine("  latest launch density: " + (launch == null || !launch.DisplayDensity.HasValue ? "(not found)" : launch.DisplayDensity.Value.ToString()));
            Console.WriteLine("  latest available sizes: " + (launch == null ? "(not found)" : LogParser.FormatDisplaySizes(launch.AvailableDisplaySizes)));
            Console.WriteLine("  latest android serial display: " + (androidSerial == null ? "(not found)" : androidSerial.DisplaySize.ToString()));
            Console.WriteLine("  latest resolution cap: " + (cap == null ? "(not found)" : cap.Cap));
            Console.WriteLine();

            if (launchMatches && serialMatches)
            {
                Console.WriteLine("PASS: Whiteout Survival is launching with the patched UHD portrait display.");
                return 0;
            }

            Console.WriteLine("FAIL: the latest logs do not show the patched UHD portrait launch yet.");

            if ((cap == null || !string.Equals(cap.Cap, "UltraHD2160p", StringComparison.Ordinal))
                && !patchStatus.PhenotypeOverridePresent)
            {
                Console.WriteLine("Hint: the service does not currently look UHD-enabled; rerun Patch with phenotype fallback enabled if you want to try the config override.");
            }
            else
            {
                Console.WriteLine("Hint: launch Whiteout Survival once after patching, then rerun verify.");
            }

            return 2;
        }

        private static void Restore(PlayGamesInstallLayout layout)
        {
            EnsureAdministrator();
            layout.EnsureInstallationExists();
            EnsureBackupExists(layout);

            PlayGamesServiceManager.Stop(layout);
            var startedAgain = false;
            try
            {
                File.Copy(layout.ExistingBackupServiceLibPath, layout.ServiceLibPath, true);
                File.Copy(layout.ExistingBackupServiceConfigPath, layout.ServiceConfigPath, true);
                DeleteIfExists(layout.HookTargetPath);
                DeleteIfExists(layout.LegacyHookTargetPath);

                PlayGamesServiceManager.Start(layout);
                startedAgain = true;

                Console.WriteLine("Restore complete");
                Console.WriteLine("  restored: " + layout.ServiceLibPath);
                Console.WriteLine("  restored: " + layout.ServiceConfigPath);
            }
            finally
            {
                if (!startedAgain)
                {
                    try
                    {
                        PlayGamesServiceManager.Start(layout);
                    }
                    catch
                    {
                    }
                }
            }
        }

        private static void BackupOriginalFiles(PlayGamesInstallLayout layout)
        {
            Directory.CreateDirectory(layout.BackupDirectory);

            if (!File.Exists(layout.BackupServiceLibPath))
            {
                var sourcePath = layout.HasLegacyBackup
                    ? layout.LegacyBackupServiceLibPath
                    : layout.ServiceLibPath;
                File.Copy(sourcePath, layout.BackupServiceLibPath, false);
            }

            if (!File.Exists(layout.BackupServiceConfigPath))
            {
                var sourcePath = layout.HasLegacyBackup
                    ? layout.LegacyBackupServiceConfigPath
                    : layout.ServiceConfigPath;
                File.Copy(sourcePath, layout.BackupServiceConfigPath, false);
            }
        }

        private static void ApplyPhenotypeFallback(PlayGamesInstallLayout layout)
        {
            var document = XDocument.Load(layout.ServiceConfigPath);
            var setting = document
                .Descendants("setting")
                .FirstOrDefault(element => string.Equals(
                    (string)element.Attribute("name"),
                    GpgConstants.PhenotypeSettingName,
                    StringComparison.Ordinal));

            if (setting == null)
            {
                throw new FriendlyException("Could not find PhenotypeFlagOverrideJson in Service.exe.config.");
            }

            var valueElement = setting.Element("value");
            if (valueElement == null)
            {
                throw new FriendlyException("PhenotypeFlagOverrideJson is missing its <value> node in Service.exe.config.");
            }

            valueElement.Value =
                "{\"Enable4KUhdResolution\":true,\"GoldTierDefaultToUse4KUhd\":true,\"SilverTierDefaultToUse4KUhd\":true}";

            document.Save(layout.ServiceConfigPath);
        }

        private static void EnsureSupportedVersion(PlayGamesInstallLayout layout)
        {
            var installedVersion = PatchStatusInspector.GetInstalledVersion(layout);
            if (!string.Equals(installedVersion, GpgConstants.SupportedVersion, StringComparison.Ordinal))
            {
                throw new FriendlyException(
                    "GPG Patcher is pinned to Google Play Games " + GpgConstants.SupportedVersion
                    + ", but the installed version is " + installedVersion + ".");
            }
        }

        private static void EnsureBackupExists(PlayGamesInstallLayout layout)
        {
            if (!layout.HasCurrentBackup && !layout.HasLegacyBackup)
            {
                throw new FriendlyException(
                    "Backup files were not found in '" + layout.BackupDirectory + "' or '" + layout.LegacyBackupDirectory + "'.");
            }
        }

        private static void DeleteIfExists(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        private static void EnsureAdministrator()
        {
            using (var identity = WindowsIdentity.GetCurrent())
            {
                var principal = new WindowsPrincipal(identity);
                if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
                {
                    throw new FriendlyException("Patch and restore require an elevated administrator shell.");
                }
            }
        }

        private static bool IsHelp(string argument)
        {
            return string.Equals(argument, "help", StringComparison.OrdinalIgnoreCase)
                || string.Equals(argument, "--help", StringComparison.OrdinalIgnoreCase)
                || string.Equals(argument, "-h", StringComparison.OrdinalIgnoreCase)
                || string.Equals(argument, "/?", StringComparison.OrdinalIgnoreCase);
        }

        private static string FormatTimestamp(DateTimeOffset? timestamp)
        {
            return timestamp.HasValue ? timestamp.Value.ToString("u") : "(unknown)";
        }

        private static void PrintUsage()
        {
            Console.WriteLine("GPG Patcher maintenance command host");
            Console.WriteLine();
            Console.WriteLine("Commands");
            Console.WriteLine("  inspect");
            Console.WriteLine("    Report Google Play Games version, patch status, and the latest logged Whiteout Survival launch settings.");
            Console.WriteLine("  patch [--phenotype-fallback]");
            Console.WriteLine("    Back up files, apply the host-side IL patch, optionally force the phenotype override, and restart the Play Games service.");
            Console.WriteLine("  verify");
            Console.WriteLine("    Read the latest logs and confirm whether Whiteout Survival launched at 1216x2160 with scaled density.");
            Console.WriteLine("  restore");
            Console.WriteLine("    Restore original files from backup and restart the Play Games service.");
        }
    }
}
