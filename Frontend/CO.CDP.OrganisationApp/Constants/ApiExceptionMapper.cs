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
            ErrorCodes.INVITE_EMAIL_ALREADY_EXISTS_FOR_ORGANISATION => ErrorMessagesList.DuplicateInviteEmail,
            ErrorCodes.ARGUMENT_NULL => ErrorMessagesList.PayLoadIssueOrNullAurgument,
            ErrorCodes.INVALID_OPERATION => ErrorMessagesList.OrganisationCreationFailed,
            ErrorCodes.PERSON_DOES_NOT_EXIST => ErrorMessagesList.PersonNotFound,
            ErrorCodes.UNPROCESSABLE_ENTITY => ErrorMessagesList.UnprocessableEntity,
            ErrorCodes.UNKNOWN_ORGANISATION => ErrorMessagesList.UnknownOrganisation,
            ErrorCodes.BUYER_INFO_NOT_EXISTS => ErrorMessagesList.BuyerInfoNotExists,
            ErrorCodes.UNKNOWN_BUYER_INFORMATION_UPDATE_TYPE => ErrorMessagesList.UnknownBuyerInformationUpdateType,
            ErrorCodes.ORGANISATION_UPDATE_INVALID_INPUT => ErrorMessagesList.OrganisationInvalidInput,
            ErrorCodes.ORGANISATION_MISSING_NAME => ErrorMessagesList.MissingOrganisationName,
            ErrorCodes.ORGANISATION_MISSING_ROLES => ErrorMessagesList.MissingRoles,
            ErrorCodes.ORGANISATION_MISSING_CONTACTPOINT => ErrorMessagesList.MissingContactPoint,
            ErrorCodes.ORGANISATION_NOPRIMARY_IDENTIFIER => ErrorMessagesList.NoPrimaryIdentifier,
            ErrorCodes.ORGANISATION_MISSING_EMAIL => ErrorMessagesList.MissingOrganisationEmail,
            ErrorCodes.ORGANISATION_EMAIL_DOES_NOT_EXISTS => ErrorMessagesList.OrganisationEmailDoesNotExist,
            ErrorCodes.ORGANISATION_MISSING_ADDRESS => ErrorMessagesList.MissingOrganisationAddress,
            ErrorCodes.ORGANISATION_MISSING_REGISTERED_ADDRESS => ErrorMessagesList.MissingOrganisationRegisteredAddress,
            ErrorCodes.ORGANISATION_MISSING_ADDITIONAL_IDENTIFIERS => ErrorMessagesList.MissingAdditionalIdentifiers,
            ErrorCodes.ORGANISATION_MISSING_IDENTIFIER_NUMBER => ErrorMessagesList.MissingIdentifierNumber,
            ErrorCodes.ORGANISATION_IDENTIFIER_NUMBER_ALREADY_EXISTS => ErrorMessagesList.IdentiferNumberAlreadyExists,
            _ => ErrorMessagesList.UnexpectedError
        };
    }
}