using CO.CDP.DataSharing.WebApiClient;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CO.CDP.OrganisationApp.Constants;

public static class DataSharingApiExceptionMapper
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
            ErrorCodes.DATASHARING_SHARE_CODE_NOT_FOUND => ErrorMessagesList.SharecodeNotFound,
            _ => ErrorMessagesList.UnexpectedError
        };
    }
}