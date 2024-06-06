namespace CO.CDP.OrganisationApp.Constants;

public static class ErrorCodes
{
    // Status400
    public const string ORGANISATION_ALREADY_EXISTS = "ORGANISATION_ALREADY_EXISTS";
    public const string INVALID_OPERATION = "INVALID_OPERATION";
    public const string ARGUMENT_NULL = "ARGUMENT_NULL";
    public const string PERSON_ALREADY_EXISTS = "PERSON_ALREADY_EXISTS";

    // Status404
    public const string PERSON_DOES_NOT_EXIST = "PERSON_DOES_NOT_EXIST";

    // Status422
    public const string UNPROCESSABLE_ENTITY = "UNPROCESSABLE_ENTITY";

    // Status500
    public const string GENERIC_ERROR = "GENERIC_ERROR";
}
