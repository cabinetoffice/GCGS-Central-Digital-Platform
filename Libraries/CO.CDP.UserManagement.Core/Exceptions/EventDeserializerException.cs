using System;

namespace CO.CDP.UserManagement.Core.Exceptions
{
    public class EventDeserializerException : Exception
    {
        public EventDeserializerException(string message, Exception? cause = null) : base(message, cause) { }
    }
}
