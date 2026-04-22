using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.UserManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RebackfillUserOrganisationMembershipsFromOrganisationInformation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Re-runs the membership backfill to pick up any organisation_person rows that were
            // missing from the initial backfill (e.g. a first owner whose sync was not captured).
            //
            // Role mapping (organisation_role_id FK to user_management.organisation_roles):
            //   - ADMIN  -> 3 (Owner)
            //   - EDITOR -> 1 (Member)
            //   - VIEWER -> 1 (Member)
            //   - Any other / unexpected scope -> 1 (Member)
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
                                organisation_role_id,
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
                                WHEN COALESCE(op.scopes, '[]'::jsonb) @> '["ADMIN"]'::jsonb THEN 3
                                ELSE 1
                            END,
                            TRUE,
                            NOW(),
                            NULL,
                            FALSE,
                            NOW(),
                            'migration:rebackfill-person-memberships'
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
