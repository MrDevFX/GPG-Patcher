using System;

namespace GpgPatcher.Gui
{
    internal sealed class InspectSummary
    {
        public string Version { get; set; }

        public string Compatible { get; set; }

        public string ServiceLibPatched { get; set; }

        public string HookDllPresent { get; set; }

        public string BackupPresent { get; set; }

        public string Density { get; set; }

        public string DisplaySize { get; set; }

        public string GuestDisplay { get; set; }

        public string ResolutionCap { get; set; }

        public string AvailableSettingsHook { get; set; }

        public string LaunchSettingsHook { get; set; }

        public string PhenotypeOverridePresent { get; set; }

        public bool IsCompatible
        {
            get { return IsTruthy(Compatible); }
        }

        public bool IsPatched
        {
            get
            {
                return IsTruthy(ServiceLibPatched)
                    && IsTruthy(HookDllPresent)
                    && IsTruthy(AvailableSettingsHook)
                    && IsTruthy(LaunchSettingsHook);
            }
        }

        public bool HasBackup
        {
            get { return IsTruthy(BackupPresent); }
        }

        public bool HasPhenotypeOverride
        {
            get { return IsTruthy(PhenotypeOverridePresent); }
        }

        public static InspectSummary Parse(string output)
        {
            var summary = new InspectSummary();
            if (string.IsNullOrWhiteSpace(output))
            {
                return summary;
            }

            var lines = output.Replace("\r\n", "\n").Split('\n');
            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();
                if (line.Length == 0)
                {
                    continue;
                }

                ReadValue(line, "version:", value => summary.Version = value);
                ReadValue(line, "compatible:", value => summary.Compatible = value);
                ReadValue(line, "service lib patched:", value => summary.ServiceLibPatched = value);
                ReadValue(line, "available-settings hook:", value => summary.AvailableSettingsHook = value);
                ReadValue(line, "launch-settings hook:", value => summary.LaunchSettingsHook = value);
                ReadValue(line, "hook dll present:", value => summary.HookDllPresent = value);
                ReadValue(line, "backup present:", value => summary.BackupPresent = value);
                ReadValue(line, "phenotype override present:", value => summary.PhenotypeOverridePresent = value);
                ReadValue(line, "density:", value => summary.Density = value);
                ReadValue(line, "display size:", value => summary.DisplaySize = value);
                ReadValue(line, "android serial display:", value => summary.GuestDisplay = value);
                ReadValue(line, "resolution cap:", value => summary.ResolutionCap = value);
            }

            return summary;
        }

        private static void ReadValue(string line, string prefix, Action<string> setter)
        {
            if (!line.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            setter(line.Substring(prefix.Length).Trim());
        }

        private static bool IsTruthy(string value)
        {
            return string.Equals(value, "True", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "Yes", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "On", StringComparison.OrdinalIgnoreCase);
        }
    }
}
