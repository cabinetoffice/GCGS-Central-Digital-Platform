using static CO.CDP.EntityVerification.UseCase.LookupIdentifierUseCase.LookupIdentifierException;

namespace CO.CDP.EntityVerification;

public static class ErrorCodes
{
    public static readonly Dictionary<Type, (int, string)> ExceptionMap = new()
    {
        { typeof(BadHttpRequestException), (StatusCodes.Status422UnprocessableEntity, "UNPROCESSABLE_ENTITY") },
        { typeof(InvalidIdentifierFormatException), (StatusCodes.Status400BadRequest, "INVALID_IDENTIFIER_FORMAT") },
    };

    public static Dictionary<string, List<string>> HttpStatusCodeErrorMap() =>
        ExceptionMap.Values
            .GroupBy(s => s.Item1)
            .ToDictionary(k => k.Key.ToString(), v => v.Select(i => i.Item2).Distinct().ToList());
}