using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ConsolidateExistingTenantsNamesAndIdentifiersNamesWithOrganisationsNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                DO $$
                BEGIN
                  CREATE TEMP TABLE OrgTenant AS (
                    SELECT o.name AS orgName, o.id AS orgId, t.id AS tenantId
                    FROM organisations o
                    JOIN tenants t ON t.id = o.tenant_id
                    JOIN identifiers i ON i.organisation_id = o.id
                    WHERE t.name <> o.name OR i.legal_name <> o.name
                  );

                  UPDATE tenants t
                  SET name = OrgTenant.orgName
                  FROM OrgTenant
                  WHERE t.id = OrgTenant.tenantId;

                  UPDATE identifiers i
                  SET legal_name = OrgTenant.orgName
                  FROM OrgTenant
                  WHERE i.organisation_id = OrgTenant.orgId;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
