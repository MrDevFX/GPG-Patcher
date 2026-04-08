using System;

namespace GpgPatcher
{
    internal sealed class FriendlyException : Exception
    {
        public FriendlyException(string message)
            : base(message)
        {
        }
    }
}
