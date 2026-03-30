using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    [DbContext(typeof(OrganisationInformationContext))]
    [Migration("20260323183057_SeedUserManagementServiceKey")]
    public class SeedUserManagementServiceKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var serviceKey = Environment.GetEnvironmentVariable("ServiceKey__ApiKey");

            if (!string.IsNullOrWhiteSpace(serviceKey))
            {
                migrationBuilder.Sql($@"
                    DO $$
                    BEGIN
                        IF EXISTS (SELECT 1 FROM authentication_keys WHERE key = '{serviceKey}') THEN
                            UPDATE authentication_keys
                            SET scopes = '[""read:organisation_data"",""write:organisation_data""]'::jsonb,
                                updated_on = NOW()
                            WHERE key = '{serviceKey}';
                        ELSE
                            INSERT INTO authentication_keys (name, key, organisation_id, revoked, scopes, created_on, updated_on)
                            VALUES ('user-management-service-key', '{serviceKey}', NULL, false, '[""read:organisation_data"",""write:organisation_data""]'::jsonb, NOW(), NOW());
                        END IF;
                    END $$;
                ");
            }

            var keyExclusion = string.IsNullOrWhiteSpace(serviceKey) ? "" : $@"AND key <> '{serviceKey}'";

            migrationBuilder.Sql($@"
                DO $$
                BEGIN
                    UPDATE authentication_keys
                    SET scopes = CASE
                        WHEN COALESCE(scopes, '[]'::jsonb) @> '[""read:organisation_data""]'::jsonb
                            THEN scopes
                        ELSE COALESCE(scopes, '[]'::jsonb) || '[""read:organisation_data""]'::jsonb
                    END
                    WHERE organisation_id IS NULL
                      {keyExclusion};
                END $$;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // no-op Down: original key state cannot be reliably restored
        }
    }
}
