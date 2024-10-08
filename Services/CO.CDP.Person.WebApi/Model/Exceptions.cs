namespace CO.CDP.Person.WebApi.Model;

public class UnknownPersonException(string message, Exception? cause = null) : Exception(message, cause);
public class UnknownPersonInviteException(string message, Exception? cause = null) : Exception(message, cause);
public class PersonInviteAlreadyClaimedException(string message, Exception? cause = null) : Exception(message, cause);
