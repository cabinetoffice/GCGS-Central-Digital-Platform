using Dapper;
using Npgsql;
using System.Data;

namespace E2ETests.Utilities;

public static class DatabaseUtility
{
    private static readonly string DatabaseConnectionString = ConfigUtility.DatabaseConnectionString();

    public static async Task MakeUserSuperAdminAsync(string email)
    {
        using IDbConnection db = new NpgsqlConnection(DatabaseConnectionString);

        var sql = $@"UPDATE persons
                    SET scopes = array_append(scopes, 'SUPERADMIN')
                    WHERE email = '{email}' AND NOT ('SUPERADMIN' = ANY(scopes));";

        await db.ExecuteAsync(sql);
    }
}