using System;

namespace CO.CDP.UserManagement.Core.Exceptions
{
    public class UnknownEventException : EventDeserializerException
    {
        public UnknownEventException(string message, Exception? cause = null) : base(message, cause) { }
    }
}
