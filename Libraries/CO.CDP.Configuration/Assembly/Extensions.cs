namespace CO.CDP.Configuration.Assembly;

public static class Extensions
{
    public static bool IsRunAs(this System.Reflection.Assembly? assembly, string entryAssemblyName)
    {
        var entryAssembly = assembly?.FullName;
        return !string.IsNullOrWhiteSpace(entryAssembly) &&
               entryAssembly.Contains(entryAssemblyName, StringComparison.InvariantCultureIgnoreCase);
    }
}