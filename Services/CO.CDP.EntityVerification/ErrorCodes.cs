using static CO.CDP.EntityVerification.UseCase.LookupIdentifierUseCase.LookupIdentifierException;
using static CO.CDP.EntityVerification.UseCase.GetCountryIdentifiersUseCase.GetCountryIdentifiersException;


namespace CO.CDP.EntityVerification;

public static class ErrorCodes
{
    public static readonly Dictionary<Type, (int, string)> Exception4xxMap = new()
    {
        { typeof(BadHttpRequestException), (StatusCodes.Status422UnprocessableEntity, "UNPROCESSABLE_ENTITY") },
        { typeof(InvalidIdentifierFormatException), (StatusCodes.Status400BadRequest, "INVALID_IDENTIFIER_FORMAT") },
        { typeof(NotFoundException), (StatusCodes.Status400BadRequest, "IDENTIFIERS_NOT_FOUND") },
        { typeof(InvalidInputException), (StatusCodes.Status400BadRequest, "INVALID_INPUT") },
    };
}