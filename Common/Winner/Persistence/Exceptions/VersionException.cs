using System;

namespace Winner.Persistence.Exceptions
{
    public class VersionException : Exception
    {
        public VersionException(string message)
            : base(message)
        {

        }
    }
}
