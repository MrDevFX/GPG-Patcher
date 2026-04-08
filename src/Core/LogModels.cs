using System;
using System.Collections.Generic;

namespace GpgPatcher
{
    internal sealed class DisplaySizeSnapshot
    {
        public DisplaySizeSnapshot(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public int Width { get; }

        public int Height { get; }

        public long Area
        {
            get { return (long)Width * Height; }
        }

        public override string ToString()
        {
            return Width + "x" + Height;
        }
    }

    internal sealed class ServiceLaunchLogEntry
    {
        public ServiceLaunchLogEntry(
            DateTimeOffset? timestamp,
            int? displayDensity,
            DisplaySizeSnapshot displaySize,
            IReadOnlyList<DisplaySizeSnapshot> availableDisplaySizes,
            int? displayId,
            string rawLine)
        {
            Timestamp = timestamp;
            DisplayDensity = displayDensity;
            DisplaySize = displaySize;
            AvailableDisplaySizes = availableDisplaySizes;
            DisplayId = displayId;
            RawLine = rawLine;
        }

        public DateTimeOffset? Timestamp { get; }

        public int? DisplayDensity { get; }

        public DisplaySizeSnapshot DisplaySize { get; }

        public IReadOnlyList<DisplaySizeSnapshot> AvailableDisplaySizes { get; }

        public int? DisplayId { get; }

        public string RawLine { get; }
    }

    internal sealed class ResolutionCapLogEntry
    {
        public ResolutionCapLogEntry(DateTimeOffset? timestamp, string cap, string rawLine)
        {
            Timestamp = timestamp;
            Cap = cap;
            RawLine = rawLine;
        }

        public DateTimeOffset? Timestamp { get; }

        public string Cap { get; }

        public string RawLine { get; }
    }

    internal sealed class AndroidSerialDisplayEntry
    {
        public AndroidSerialDisplayEntry(DisplaySizeSnapshot displaySize, string rawLine)
        {
            DisplaySize = displaySize;
            RawLine = rawLine;
        }

        public DisplaySizeSnapshot DisplaySize { get; }

        public string RawLine { get; }
    }

    internal sealed class PatchStatus
    {
        public bool AvailableSettingsPatched { get; set; }

        public bool LaunchSettingsPatched { get; set; }

        public bool HookAssemblyReferencePresent { get; set; }

        public bool HookDllPresent { get; set; }

        public bool BackupPresent { get; set; }

        public bool PhenotypeOverridePresent { get; set; }

        public string PhenotypeOverrideValue { get; set; }

        public bool IsPatched
        {
            get
            {
                return AvailableSettingsPatched
                    && LaunchSettingsPatched
                    && HookAssemblyReferencePresent
                    && HookDllPresent;
            }
        }
    }
}
