namespace CO.CDP.WebApi.Foundation;

public static class ErrorCodesExtension
{
    public static Dictionary<string, List<string>> HttpStatusCodeErrorMap
        (this Dictionary<Type, (int, string)> exceptionMap)
    {
        return exceptionMap.Values
             .GroupBy(s => s.Item1)
             .ToDictionary(k => k.Key.ToString(), v => v.Select(i => i.Item2).Distinct().ToList());
    }
}