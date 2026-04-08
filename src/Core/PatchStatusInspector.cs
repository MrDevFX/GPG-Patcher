using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using dnlib.DotNet;

namespace GpgPatcher
{
    internal static class PatchStatusInspector
    {
        public static string GetInstalledVersion(PlayGamesInstallLayout layout)
        {
            layout.EnsureInstallationExists();
            var version = System.Diagnostics.FileVersionInfo.GetVersionInfo(layout.ServiceExePath).FileVersion;
            return string.IsNullOrWhiteSpace(version) ? string.Empty : version.Trim();
        }

        public static PatchStatus Inspect(PlayGamesInstallLayout layout)
        {
            layout.EnsureInstallationExists();

            var patchStatus = new PatchStatus
            {
                HookDllPresent = File.Exists(layout.HookTargetPath) || File.Exists(layout.LegacyHookTargetPath),
                BackupPresent = layout.HasCurrentBackup || layout.HasLegacyBackup,
            };

            using (var module = ModuleDefMD.Load(layout.ServiceLibPath))
            {
                var serviceType = module.Types.FirstOrDefault(type => type.FullName == GpgConstants.ServiceTypeName);
                if (serviceType == null)
                {
                    throw new FriendlyException("Could not find AppSessionScope in ServiceLib.dll.");
                }

                var availableMethod = ServiceLibPatcher.FindTargetMethod(serviceType, GpgConstants.AvailableSettingsMethodName);
                var launchMethod = ServiceLibPatcher.FindTargetMethod(serviceType, GpgConstants.LaunchSettingsMethodName);

                patchStatus.AvailableSettingsPatched = ServiceLibPatcher.HasAnyHookCall(
                    availableMethod,
                    GpgConstants.PatchAvailableSettingsMethod);
                patchStatus.LaunchSettingsPatched = ServiceLibPatcher.HasAnyHookCall(
                    launchMethod,
                    GpgConstants.PatchAndroidDisplaySettingsMethod);
                patchStatus.HookAssemblyReferencePresent = module.GetAssemblyRefs()
                    .Any(reference =>
                        string.Equals(reference.Name, GpgConstants.HookAssemblyName, StringComparison.Ordinal)
                        || string.Equals(reference.Name, GpgConstants.LegacyHookAssemblyName, StringComparison.Ordinal));
            }

            patchStatus.PhenotypeOverrideValue = ReadPhenotypeOverride(layout.ServiceConfigPath);
            patchStatus.PhenotypeOverridePresent = !string.IsNullOrWhiteSpace(patchStatus.PhenotypeOverrideValue);

            return patchStatus;
        }

        private static string ReadPhenotypeOverride(string configPath)
        {
            var document = XDocument.Load(configPath);
            var setting = document
                .Descendants("setting")
                .FirstOrDefault(element => string.Equals(
                    (string)element.Attribute("name"),
                    GpgConstants.PhenotypeSettingName,
                    StringComparison.Ordinal));

            var valueElement = setting == null ? null : setting.Element("value");
            return valueElement == null ? string.Empty : valueElement.Value;
        }
    }
}
