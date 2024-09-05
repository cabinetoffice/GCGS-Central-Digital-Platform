namespace CO.CDP.Person.WebApi.Model;

public class PersonInviteAlreadyClaimedException(string message, Exception? cause = null) : Exception(message, cause);