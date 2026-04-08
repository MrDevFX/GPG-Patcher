namespace GpgPatcher
{
    internal static class GpgConstants
    {
        public const string SupportedVersion = "26.3.725.2";
        public const string TargetPackageName = "com.gof.global";
        public const int TargetWidth = 1216;
        public const int TargetHeight = 2160;
        public const string AppDataDirectoryName = "GpgPatcher";
        public const string LegacyAppDataDirectoryName = "GpgResPoC";
        public const string HookAssemblyName = "GpgPatcher.Hooks";
        public const string HookAssemblyFileName = "GpgPatcher.Hooks.dll";
        public const string HookTypeNamespace = "GpgPatcher.Hooks";
        public const string LegacyHookAssemblyName = "GpgResPoc.Hooks";
        public const string LegacyHookAssemblyFileName = "GpgResPoc.Hooks.dll";
        public const string LegacyHookTypeNamespace = "GpgResPoc.Hooks";
        public const string HookTypeName = "DisplaySettingsHooks";
        public const string PatchAvailableSettingsMethod = "PatchAvailableSettings";
        public const string PatchAndroidDisplaySettingsMethod = "PatchAndroidDisplaySettings";
        public const string ServiceTypeName = "Google.Hpe.Service.AppSession.AppSessionScope";
        public const string AvailableSettingsMethodName = "GetAvailableAndroidDisplaySettings";
        public const string LaunchSettingsMethodName = "GetAndroidDisplaySettingsOnGameLaunch";
        public const string ServiceLogFileName = "Service.log";
        public const string AndroidSerialLogFileName = "AndroidSerial.log";
        public const string PhenotypeSettingName = "PhenotypeFlagOverrideJson";
    }
}
