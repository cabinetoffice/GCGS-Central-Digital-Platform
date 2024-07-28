namespace CO.CDP.Forms.WebApi.Model;

public class UnknownOrganisationException(string message, Exception? cause = null) : Exception(message, cause);
public class UnknownSectionException(string message, Exception? cause = null) : Exception(message, cause);
public class UnknownQuestionsException(string message, Exception? cause = null) : Exception(message, cause);

public class UnknownConnectedEntityException(string message, Exception? cause = null) : Exception(message, cause);