namespace CO.CDP.OrganisationApp.Constants;

public static class ErrorMessagesList
{
    public const string DuplicatePersonName = "A person with this name already exists. Please try again.";
    public const string PersonNotFound = "The requested person was not found.";
    public const string PersonCreationFailed = "Adding a person failed, have you provided the correct person details.";

    public const string OrganisationCreationFailed = "Organsiation creation failed, have you provided the correct organisation details.";
    public const string OrganisationNotFound = "The requested organisation was not found.";
    public const string DuplicateOgranisationName = "An organisation with this name already exists. Please try again.";

    public const string PayLoadIssueOrNullAurgument = "Please make sure all the required information has been provided correctly.";

    public const string UnprocessableEntity = "An unprocessable entity found. Please try again with valid data.";
    public const string UnexpectedError = "An unexpected error occurred. Please try again with valid data.";
}