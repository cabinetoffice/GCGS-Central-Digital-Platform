using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.UserManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BackfillFindATenderAccessFromOrganisationInformation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    )
                    AND EXISTS (
                        SELECT 1 FROM information_schema.tables
                        WHERE table_schema = 'user_management' AND table_name = 'applications'
                    )
                    AND EXISTS (
                        SELECT 1 FROM information_schema.tables
                        WHERE table_schema = 'user_management' AND table_name = 'application_roles'
                    )
                    AND EXISTS (
                        SELECT 1 FROM information_schema.tables
                        WHERE table_schema = 'user_management' AND table_name = 'organisation_applications'
                    )
                    AND EXISTS (
                        SELECT 1 FROM information_schema.tables
                        WHERE table_schema = 'user_management' AND table_name = 'user_application_assignments'
                    )
                    AND EXISTS (
                        SELECT 1 FROM information_schema.tables
                        WHERE table_schema = 'user_management' AND table_name = 'user_assignment_roles'
                    ) THEN
                        -- Ensure Find a Tender is enabled for any organisation that has a qualifying
                        -- Organisation Information membership before backfilling assignments.
                        WITH source_memberships AS (
                            SELECT DISTINCT
                                membership.id AS membership_id,
                                membership.organisation_id
                            FROM public.organisation_person op
                            INNER JOIN public.persons p
                                ON p.id = op.person_id
                            INNER JOIN public.organisations oi_org
                                ON oi_org.id = op.organisation_id
                            INNER JOIN user_management.organisations um_org
                                ON um_org.cdp_organisation_guid = oi_org.guid
                            INNER JOIN user_management.user_organisation_memberships membership
                                ON membership.organisation_id = um_org.id
                               AND membership.is_active = TRUE
                               AND membership.is_deleted = FALSE
                               AND (
                                    membership.cdp_person_id = p.guid
                                    OR (
                                        p.user_urn IS NOT NULL
                                        AND btrim(p.user_urn) <> ''
                                        AND membership.user_principal_id = p.user_urn
                                    )
                               )
                            WHERE oi_org.roles && ARRAY[1, 3, 4]::integer[]
                        )
                        INSERT INTO user_management.organisation_applications
                            (
                                organisation_id,
                                application_id,
                                is_active,
                                enabled_at,
                                enabled_by,
                                is_deleted,
                                created_at,
                                created_by
                            )
                        SELECT DISTINCT
                            sm.organisation_id,
                            app.id,
                            TRUE,
                            NOW(),
                            'migration:backfill-find-a-tender-access',
                            FALSE,
                            NOW(),
                            'migration:backfill-find-a-tender-access'
                        FROM source_memberships sm
                        CROSS JOIN user_management.applications app
                        WHERE app.client_id = 'find-a-tender'
                        ON CONFLICT (organisation_id, application_id) DO UPDATE
                        SET
                            is_active = TRUE,
                            enabled_at = NOW(),
                            enabled_by = 'migration:backfill-find-a-tender-access',
                            disabled_at = NULL,
                            disabled_by = NULL,
                            is_deleted = FALSE,
                            deleted_at = NULL,
                            deleted_by = NULL,
                            modified_at = NOW(),
                            modified_by = 'migration:backfill-find-a-tender-access';

                        WITH source_memberships AS (
                            SELECT DISTINCT
                                membership.id AS membership_id,
                                membership.organisation_id,
                                CASE
                                    WHEN oi_org.roles @> ARRAY[1]::integer[] THEN TRUE
                                    ELSE FALSE
                                END AS has_buyer_role,
                                CASE
                                    WHEN oi_org.roles && ARRAY[3, 4]::integer[] THEN TRUE
                                    ELSE FALSE
                                END AS has_supplier_or_tenderer_role,
                                CASE
                                    WHEN COALESCE(op.scopes, '[]'::jsonb) @> '["ADMIN"]'::jsonb THEN TRUE
                                    ELSE FALSE
                                END AS has_admin_scope
                            FROM public.organisation_person op
                            INNER JOIN public.persons p
                                ON p.id = op.person_id
                            INNER JOIN public.organisations oi_org
                                ON oi_org.id = op.organisation_id
                            INNER JOIN user_management.organisations um_org
                                ON um_org.cdp_organisation_guid = oi_org.guid
                            INNER JOIN user_management.user_organisation_memberships membership
                                ON membership.organisation_id = um_org.id
                               AND membership.is_active = TRUE
                               AND membership.is_deleted = FALSE
                               AND (
                                    membership.cdp_person_id = p.guid
                                    OR (
                                        p.user_urn IS NOT NULL
                                        AND btrim(p.user_urn) <> ''
                                        AND membership.user_principal_id = p.user_urn
                                    )
                               )
                            WHERE oi_org.roles && ARRAY[1, 3, 4]::integer[]
                        ),
                        target_roles AS (
                            SELECT
                                sm.membership_id,
                                sm.organisation_id,
                                CASE
                                    WHEN sm.has_buyer_role AND sm.has_admin_scope THEN 'Editor (buyer)'
                                    WHEN sm.has_supplier_or_tenderer_role AND sm.has_admin_scope THEN 'Editor (supplier)'
                                    WHEN sm.has_buyer_role THEN 'Viewer (buyer)'
                                    WHEN sm.has_supplier_or_tenderer_role THEN 'Viewer (supplier)'
                                    ELSE NULL
                                END AS target_role_name
                            FROM source_memberships sm
                        ),
                        resolved_targets AS (
                            SELECT
                                tr.membership_id,
                                oa.id AS organisation_application_id,
                                ar.id AS application_role_id
                            FROM target_roles tr
                            INNER JOIN user_management.applications app
                                ON app.client_id = 'find-a-tender'
                               AND app.is_active = TRUE
                               AND app.is_deleted = FALSE
                            INNER JOIN user_management.organisation_applications oa
                                ON oa.organisation_id = tr.organisation_id
                               AND oa.application_id = app.id
                            INNER JOIN user_management.application_roles ar
                                ON ar.application_id = app.id
                               AND ar.name = tr.target_role_name
                               AND ar.is_active = TRUE
                               AND ar.is_deleted = FALSE
                            WHERE tr.target_role_name IS NOT NULL
                        )
                        INSERT INTO user_management.user_application_assignments
                            (
                                user_organisation_membership_id,
                                organisation_application_id,
                                is_active,
                                assigned_at,
                                assigned_by,
                                is_deleted,
                                created_at,
                                created_by
                            )
                        SELECT DISTINCT
                            rt.membership_id,
                            rt.organisation_application_id,
                            TRUE,
                            NOW(),
                            'migration:backfill-find-a-tender-access',
                            FALSE,
                            NOW(),
                            'migration:backfill-find-a-tender-access'
                        FROM resolved_targets rt
                        ON CONFLICT (user_organisation_membership_id, organisation_application_id) DO UPDATE
                        SET
                            is_active = TRUE,
                            assigned_at = NOW(),
                            assigned_by = 'migration:backfill-find-a-tender-access',
                            revoked_at = NULL,
                            revoked_by = NULL,
                            is_deleted = FALSE,
                            deleted_at = NULL,
                            deleted_by = NULL,
                            modified_at = NOW(),
                            modified_by = 'migration:backfill-find-a-tender-access';

                        WITH resolved_targets AS (
                            SELECT
                                uaa.id AS user_assignment_id,
                                ar.id AS target_role_id
                            FROM public.organisation_person op
                            INNER JOIN public.persons p
                                ON p.id = op.person_id
                            INNER JOIN public.organisations oi_org
                                ON oi_org.id = op.organisation_id
                            INNER JOIN user_management.organisations um_org
                                ON um_org.cdp_organisation_guid = oi_org.guid
                            INNER JOIN user_management.user_organisation_memberships membership
                                ON membership.organisation_id = um_org.id
                               AND membership.is_active = TRUE
                               AND membership.is_deleted = FALSE
                               AND (
                                    membership.cdp_person_id = p.guid
                                    OR (
                                        p.user_urn IS NOT NULL
                                        AND btrim(p.user_urn) <> ''
                                        AND membership.user_principal_id = p.user_urn
                                    )
                               )
                            INNER JOIN user_management.applications app
                                ON app.client_id = 'find-a-tender'
                               AND app.is_active = TRUE
                               AND app.is_deleted = FALSE
                            INNER JOIN user_management.organisation_applications oa
                                ON oa.organisation_id = membership.organisation_id
                               AND oa.application_id = app.id
                            INNER JOIN user_management.user_application_assignments uaa
                                ON uaa.user_organisation_membership_id = membership.id
                               AND uaa.organisation_application_id = oa.id
                            INNER JOIN user_management.application_roles ar
                                ON ar.application_id = app.id
                               AND ar.name = CASE
                                   WHEN oi_org.roles @> ARRAY[1]::integer[] AND COALESCE(op.scopes, '[]'::jsonb) @> '["ADMIN"]'::jsonb THEN 'Editor (buyer)'
                                   WHEN oi_org.roles && ARRAY[3, 4]::integer[] AND COALESCE(op.scopes, '[]'::jsonb) @> '["ADMIN"]'::jsonb THEN 'Editor (supplier)'
                                   WHEN oi_org.roles @> ARRAY[1]::integer[] THEN 'Viewer (buyer)'
                                   WHEN oi_org.roles && ARRAY[3, 4]::integer[] THEN 'Viewer (supplier)'
                                   ELSE NULL
                               END
                            WHERE oi_org.roles && ARRAY[1, 3, 4]::integer[]
                        )
                        DELETE FROM user_management.user_assignment_roles uar
                        USING resolved_targets rt,
                              user_management.application_roles ar
                        WHERE uar.user_assignment_id = rt.user_assignment_id
                          AND uar.role_id = ar.id
                          AND ar.application_id = (
                              SELECT id
                              FROM user_management.applications
                              WHERE client_id = 'find-a-tender'
                          )
                          AND uar.role_id <> rt.target_role_id;

                        WITH resolved_targets AS (
                            SELECT
                                uaa.id AS user_assignment_id,
                                ar.id AS target_role_id
                            FROM public.organisation_person op
                            INNER JOIN public.persons p
                                ON p.id = op.person_id
                            INNER JOIN public.organisations oi_org
                                ON oi_org.id = op.organisation_id
                            INNER JOIN user_management.organisations um_org
                                ON um_org.cdp_organisation_guid = oi_org.guid
                            INNER JOIN user_management.user_organisation_memberships membership
                                ON membership.organisation_id = um_org.id
                               AND membership.is_active = TRUE
                               AND membership.is_deleted = FALSE
                               AND (
                                    membership.cdp_person_id = p.guid
                                    OR (
                                        p.user_urn IS NOT NULL
                                        AND btrim(p.user_urn) <> ''
                                        AND membership.user_principal_id = p.user_urn
                                    )
                               )
                            INNER JOIN user_management.applications app
                                ON app.client_id = 'find-a-tender'
                               AND app.is_active = TRUE
                               AND app.is_deleted = FALSE
                            INNER JOIN user_management.organisation_applications oa
                                ON oa.organisation_id = membership.organisation_id
                               AND oa.application_id = app.id
                            INNER JOIN user_management.user_application_assignments uaa
                                ON uaa.user_organisation_membership_id = membership.id
                               AND uaa.organisation_application_id = oa.id
                            INNER JOIN user_management.application_roles ar
                                ON ar.application_id = app.id
                               AND ar.name = CASE
                                   WHEN oi_org.roles @> ARRAY[1]::integer[] AND COALESCE(op.scopes, '[]'::jsonb) @> '["ADMIN"]'::jsonb THEN 'Editor (buyer)'
                                   WHEN oi_org.roles && ARRAY[3, 4]::integer[] AND COALESCE(op.scopes, '[]'::jsonb) @> '["ADMIN"]'::jsonb THEN 'Editor (supplier)'
                                   WHEN oi_org.roles @> ARRAY[1]::integer[] THEN 'Viewer (buyer)'
                                   WHEN oi_org.roles && ARRAY[3, 4]::integer[] THEN 'Viewer (supplier)'
                                   ELSE NULL
                               END
                            WHERE oi_org.roles && ARRAY[1, 3, 4]::integer[]
                        )
                        INSERT INTO user_management.user_assignment_roles
                            (
                                user_assignment_id,
                                role_id
                            )
                        SELECT DISTINCT
                            rt.user_assignment_id,
                            rt.target_role_id
                        FROM resolved_targets rt
                        ON CONFLICT (user_assignment_id, role_id) DO NOTHING;
                    END IF;
                END
                $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentional no-op: backfilled Find a Tender assignments are not removed on rollback.
        }
    }
}
