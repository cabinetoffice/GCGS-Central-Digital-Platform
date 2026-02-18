using System;

namespace CO.CDP.UserManagement.Core.Exceptions
{
    public class DeserializationFailedException : EventDeserializerException
    {
        public DeserializationFailedException(string message, Exception? cause = null) : base(message, cause) { }
    }
}
