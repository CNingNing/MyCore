using System;

namespace Winner.Persistence.Exceptions
{
    public class LimitCountOverflowException : Exception
    {
        public LimitCountOverflowException(string message)
            : base(message)
        {

        }
    }
}
