using static CO.CDP.EntityVerification.UseCase.LookupIdentifierUseCase.LookupIdentifierException;
using static CO.CDP.EntityVerification.UseCase.GetIdentifierRegistriesUseCase.GetIdentifierRegistriesException;


namespace CO.CDP.EntityVerification;

public static class ErrorCodes
{
    public static readonly Dictionary<Type, (int, string)> Exception4xxMap = new()
    {
        { typeof(BadHttpRequestException), (StatusCodes.Status422UnprocessableEntity, "UNPROCESSABLE_ENTITY") },
        { typeof(InvalidIdentifierFormatException), (StatusCodes.Status400BadRequest, "INVALID_IDENTIFIER_FORMAT") },
        { typeof(NotFoundException), (StatusCodes.Status404NotFound, "IDENTIFIERS_NOT_FOUND") },
        { typeof(InvalidInputException), (StatusCodes.Status400BadRequest, "INVALID_INPUT") },
    };
}