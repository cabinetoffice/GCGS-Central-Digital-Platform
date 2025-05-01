using System.Reflection;

namespace CO.CDP.OrganisationInformation.Persistence.StoredProcedures;

public static class StoredProcedureScriptLoader
{
    public static string Load(string filename)
    {
        var fullPath = Path.Combine(AppContext.BaseDirectory, "StoredProcedures", filename);
        return File.ReadAllText(fullPath);
    }
}
