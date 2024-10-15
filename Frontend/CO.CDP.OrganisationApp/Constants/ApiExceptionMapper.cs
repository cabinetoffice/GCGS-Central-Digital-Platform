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

        if (!string.IsNullOrEmpty(code))
        {
            modelState.AddModelError(string.Empty, code switch
            {
                ErrorCodes.ORGANISATION_ALREADY_EXISTS => ErrorMessagesList.DuplicateOgranisationName,
                ErrorCodes.ARGUMENT_NULL => ErrorMessagesList.PayLoadIssueOrNullAurgument,
                ErrorCodes.INVALID_OPERATION => ErrorMessagesList.OrganisationCreationFailed,
                ErrorCodes.PERSON_DOES_NOT_EXIST => ErrorMessagesList.PersonNotFound,
                ErrorCodes.UNPROCESSABLE_ENTITY => ErrorMessagesList.UnprocessableEntity,
                ErrorCodes.UNKNOWN_ORGANISATION => ErrorMessagesList.UnknownOrganisation,
                ErrorCodes.BUYER_INFO_NOT_EXISTS => ErrorMessagesList.BuyerInfoNotExists,
                ErrorCodes.UNKNOWN_BUYER_INFORMATION_UPDATE_TYPE => ErrorMessagesList.UnknownBuyerInformationUpdateType,
                _ => ErrorMessagesList.UnexpectedError
            });
        }
    }

    private static string? ExtractErrorCode(ApiException<ProblemDetails> aex)
    {
        return aex.Result.AdditionalProperties.TryGetValue("code", out var code) && code is string codeString
            ? codeString
            : null;
    }
}
