using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    [DbContext(typeof(OrganisationInformationContext))]
    [Migration("20260323080110_FixServiceKeyScopes")]
    public class FixServiceKeyScopes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    UPDATE authentication_keys
                    SET scopes = '[""read:organisation_data"",""write:organisation_data"",""read:person_data""]'::jsonb
                    WHERE organisation_id IS NULL;
                END $$;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // no-op: original scopes cannot be reliably restored
        }
    }
}
