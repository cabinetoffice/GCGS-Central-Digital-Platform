using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class OrgNameCaseInsensitiveIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                DO $$
                BEGIN
                    DROP INDEX IF EXISTS ix_tenants_name;
                    CREATE UNIQUE INDEX ix_tenants_name ON tenants(UPPER(name));

                    DROP INDEX IF EXISTS ix_organisations_name;
                    CREATE UNIQUE INDEX ix_organisations_name ON organisations(UPPER(name));
                END $$;
             ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                DO $$
                BEGIN
                    DROP INDEX IF EXISTS ix_tenants_name;
                    CREATE UNIQUE INDEX ix_tenants_name ON tenants(name);

                    DROP INDEX IF EXISTS ix_organisations_name;
                    CREATE UNIQUE INDEX ix_organisations_name ON organisations(name);
                END $$;
             ");
        }
    }
}
