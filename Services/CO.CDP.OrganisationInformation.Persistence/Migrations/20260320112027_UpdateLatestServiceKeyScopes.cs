using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    [DbContext(typeof(OrganisationInformationContext))]
    [Migration("20260320112027_UpdateLatestServiceKeyScopes")]
    public class UpdateLatestServiceKeyScopes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"
                DO $$
                BEGIN
                    UPDATE authentication_keys
                    SET scopes = '[""read:person_data"",""write:person_data"",""read:organisation_data"",""write:organisation_data""]'::jsonb
                    WHERE id = (
                        SELECT id FROM authentication_keys
                        WHERE organisation_id IS NULL
                        ORDER BY created_on DESC, id DESC
                        LIMIT 1
                    );
                END $$;
            ";

            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // no-op Down: original scopes cannot be reliably restored
        }
    }
}
