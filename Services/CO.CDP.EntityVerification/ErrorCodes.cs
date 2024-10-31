using static CO.CDP.EntityVerification.UseCase.LookupIdentifierUseCase.LookupIdentifierException;

namespace CO.CDP.EntityVerification;

public static class ErrorCodes
{
    public static readonly Dictionary<Type, (int, string)> Exception4xxMap = new()
    {
        { typeof(BadHttpRequestException), (StatusCodes.Status422UnprocessableEntity, "UNPROCESSABLE_ENTITY") },
        { typeof(InvalidIdentifierFormatException), (StatusCodes.Status400BadRequest, "INVALID_IDENTIFIER_FORMAT") },
    };
}