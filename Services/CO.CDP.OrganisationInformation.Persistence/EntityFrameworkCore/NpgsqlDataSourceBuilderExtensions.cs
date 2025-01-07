using Npgsql;

namespace CO.CDP.OrganisationInformation.Persistence;

public static class NpgsqlDataSourceBuilderExtensions
{
    public static NpgsqlDataSourceBuilder MapEnums(this NpgsqlDataSourceBuilder npgsqlDataSourceBuilder)
    {
        npgsqlDataSourceBuilder.MapEnum<OrganisationRelationship>();
        return npgsqlDataSourceBuilder;
    }
}