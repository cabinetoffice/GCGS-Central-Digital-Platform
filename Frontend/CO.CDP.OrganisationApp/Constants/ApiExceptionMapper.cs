using CO.CDP.Organisation.WebApiClient;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CO.CDP.OrganisationApp.Constants;

public static class ApiExceptionMapper
{
    public static void MapApiExceptions(
        ApiException<ProblemDetails> aex,
        ModelStateDictionary modelState)
    {
        var code = ExtractErrorCode(aex);

        if (string.IsNullOrWhiteSpace(code))
        {
            modelState.AddModelError(string.Empty, ErrorMessagesList.UnexpectedError);
            return;
        }

        var errorMessage = GetErrorMessageByCode(code);
        modelState.AddModelError(string.Empty, errorMessage);

    }

    private static string? ExtractErrorCode(ApiException<ProblemDetails> aex)
    {
        return aex.Result.AdditionalProperties.TryGetValue("code", out var code) && code is string codeString
            ? codeString
            : null;
    }

    private static string GetErrorMessageByCode(string errorCode)
    {
        return errorCode switch
        {
            ErrorCodes.ORGANISATION_ALREADY_EXISTS => ErrorMessagesList.DuplicateOgranisationName,
            ErrorCodes.EMAIL_ALREADY_EXISTS_WITHIN_ORGANISATION => ErrorMessagesList.DuplicatePersonEmail,
            ErrorCodes.ARGUMENT_NULL => ErrorMessagesList.PayLoadIssueOrNullAurgument,
            ErrorCodes.INVALID_OPERATION => ErrorMessagesList.OrganisationCreationFailed,
            ErrorCodes.PERSON_DOES_NOT_EXIST => ErrorMessagesList.PersonNotFound,
            ErrorCodes.UNPROCESSABLE_ENTITY => ErrorMessagesList.UnprocessableEntity,
            ErrorCodes.UNKNOWN_ORGANISATION => ErrorMessagesList.UnknownOrganisation,
            ErrorCodes.BUYER_INFO_NOT_EXISTS => ErrorMessagesList.BuyerInfoNotExists,
            ErrorCodes.UNKNOWN_BUYER_INFORMATION_UPDATE_TYPE => ErrorMessagesList.UnknownBuyerInformationUpdateType,
            ErrorCodes.PERSON_ALREADY_ADDED_TO_ORGANISATION => ErrorMessagesList.AlreadyMemberOfOrganisation,
            _ => ErrorMessagesList.UnexpectedError
        };
    }
}