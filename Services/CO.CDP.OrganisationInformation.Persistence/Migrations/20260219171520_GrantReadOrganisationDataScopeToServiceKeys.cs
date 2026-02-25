using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    [DbContext(typeof(OrganisationInformationContext))]
    [Migration("20260219171520_GrantReadOrganisationDataScopeToServiceKeys")]
    public class GrantReadOrganisationDataScopeToServiceKeys : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"
                DO $$
                BEGIN
                    UPDATE authentication_keys
                    SET scopes = CASE
                        WHEN COALESCE(scopes, '[]'::jsonb) @> '[""read:organisation_data""]'::jsonb
                            THEN COALESCE(scopes, '[]'::jsonb)
                        ELSE COALESCE(scopes, '[]'::jsonb) || '[""read:organisation_data""]'::jsonb
                    END
                    WHERE organisation_id IS NULL;
                END $$;
            ";

            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var sql = @"
                DO $$
                BEGIN
                    UPDATE authentication_keys AS ak
                    SET scopes = COALESCE(
                        (
                            SELECT jsonb_agg(scope)
                            FROM jsonb_array_elements_text(COALESCE(ak.scopes, '[]'::jsonb)) AS scope
                            WHERE scope <> 'read:organisation_data'
                        ),
                        '[]'::jsonb
                    )
                    WHERE ak.organisation_id IS NULL
                      AND COALESCE(ak.scopes, '[]'::jsonb) @> '[""read:organisation_data""]'::jsonb;
                END $$;
            ";

            migrationBuilder.Sql(sql);
        }
    }
}
