using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.UserManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BackfillUserOrganisationMembershipsFromOrganisationInformation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Role mapping:
            //   - ADMIN  -> Owner
            //   - EDITOR -> Member
            //   - VIEWER -> Member
            //   - Any other / unexpected scope -> Member
            //
            // Rows are skipped when:
            //   - the source person has no user_urn
            //   - there is no matching user_management organisation for the source org GUID
            //
            // Idempotency: conflict on (user_principal_id, organisation_id) does nothing.
            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.tables
                        WHERE table_schema = 'public' AND table_name = 'organisation_person'
                    )
                    AND EXISTS (
                        SELECT 1 FROM information_schema.tables
                        WHERE table_schema = 'public' AND table_name = 'persons'
                    )
                    AND EXISTS (
                        SELECT 1 FROM information_schema.tables
                        WHERE table_schema = 'public' AND table_name = 'organisations'
                    )
                    AND EXISTS (
                        SELECT 1 FROM information_schema.tables
                        WHERE table_schema = 'user_management' AND table_name = 'organisations'
                    )
                    AND EXISTS (
                        SELECT 1 FROM information_schema.tables
                        WHERE table_schema = 'user_management' AND table_name = 'user_organisation_memberships'
                    ) THEN
                        INSERT INTO user_management.user_organisation_memberships
                            (
                                user_principal_id,
                                cdp_person_id,
                                organisation_id,
                                organisation_role,
                                is_active,
                                joined_at,
                                invited_by,
                                is_deleted,
                                created_at,
                                created_by
                            )
                        SELECT
                            p.user_urn,
                            p.guid,
                            um_org.id,
                            CASE
                                WHEN COALESCE(op.scopes, '[]'::jsonb) @> '["ADMIN"]'::jsonb THEN 'Owner'
                                ELSE 'Member'
                            END,
                            TRUE,
                            NOW(),
                            NULL,
                            FALSE,
                            NOW(),
                            'migration:backfill-person-memberships'
                        FROM public.organisation_person op
                        INNER JOIN public.persons p
                            ON p.id = op.person_id
                        INNER JOIN public.organisations oi_org
                            ON oi_org.id = op.organisation_id
                        INNER JOIN user_management.organisations um_org
                            ON um_org.cdp_organisation_guid = oi_org.guid
                        WHERE p.user_urn IS NOT NULL
                          AND btrim(p.user_urn) <> ''
                        ON CONFLICT (user_principal_id, organisation_id) DO NOTHING;
                    END IF;
                END
                $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentional no-op: backfilled memberships are not removed on rollback.
        }
    }
}
