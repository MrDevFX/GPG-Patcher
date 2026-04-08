namespace GpgPatcher.Gui
{
    internal sealed class CommandResult
    {
        public int ExitCode { get; set; }

        public string StandardOutput { get; set; }

        public string StandardError { get; set; }

        public bool Success
        {
            get { return ExitCode == 0; }
        }
    }
}
