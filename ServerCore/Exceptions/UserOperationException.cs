using System;

namespace ServerCore.Exceptions
{
    /// <summary>
    /// Specialized operations that demonstrate user error and should generally be surfaced to the user because it is something they can fix.
    /// </summary>
    public class UserOperationException : Exception
    {
        public UserOperationException()
        {
        }

        public UserOperationException(string message)
            : base(message)
        {
        }

        public UserOperationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
