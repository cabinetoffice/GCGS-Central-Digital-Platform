using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.UserManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BackfillInviteRolesFromOrganisationInformation : Migration
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
                        WHERE table_schema = 'public' AND table_name = 'person_invites'
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
                        WHERE table_schema = 'user_management' AND table_name = 'organisation_roles'
                    )
                    AND EXISTS (
                        SELECT 1 FROM information_schema.tables
                        WHERE table_schema = 'user_management' AND table_name = 'invite_role_mappings'
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
                        WHERE table_schema = 'user_management' AND table_name = 'invite_role_application_assignments'
                    ) THEN
                        WITH source_invites AS (
                            SELECT DISTINCT
                                pi.guid AS cdp_person_invite_guid,
                                um_org.id AS organisation_id,
                                CASE
                                    WHEN COALESCE(pi.scopes, ARRAY[]::text[]) @> ARRAY['ADMIN']::text[] THEN 3
                                    ELSE 1
                                END AS organisation_role_id
                            FROM public.person_invites pi
                            INNER JOIN public.organisations oi_org
                                ON oi_org.id = pi.organisation_id
                            INNER JOIN user_management.organisations um_org
                                ON um_org.cdp_organisation_guid = oi_org.guid
                            WHERE pi.person_id IS NULL
                              AND (pi.expires_on IS NULL OR pi.expires_on >= NOW())
                        )
                        INSERT INTO user_management.invite_role_mappings
                            (
                                cdp_person_invite_guid,
                                organisation_id,
                                organisation_role_id,
                                is_deleted,
                                created_at,
                                created_by
                            )
                        SELECT
                            si.cdp_person_invite_guid,
                            si.organisation_id,
                            si.organisation_role_id,
                            FALSE,
                            NOW(),
                            'migration:backfill-invite-roles'
                        FROM source_invites si
                        ON CONFLICT (cdp_person_invite_guid) DO UPDATE
                        SET
                            organisation_id = EXCLUDED.organisation_id,
                            organisation_role_id = EXCLUDED.organisation_role_id,
                            is_deleted = FALSE,
                            deleted_at = NULL,
                            deleted_by = NULL,
                            modified_at = NOW(),
                            modified_by = 'migration:backfill-invite-roles';

                        WITH qualifying_invites AS (
                            SELECT DISTINCT
                                pi.guid AS cdp_person_invite_guid,
                                um_org.id AS organisation_id,
                                CASE
                                    WHEN oi_org.roles @> ARRAY[1]::integer[] THEN TRUE
                                    ELSE FALSE
                                END AS has_buyer_role,
                                CASE
                                    WHEN oi_org.roles && ARRAY[3, 4]::integer[] THEN TRUE
                                    ELSE FALSE
                                END AS has_supplier_or_tenderer_role,
                                CASE
                                    WHEN COALESCE(pi.scopes, ARRAY[]::text[]) @> ARRAY['ADMIN']::text[] THEN TRUE
                                    ELSE FALSE
                                END AS has_admin_scope,
                                CASE
                                    WHEN COALESCE(pi.scopes, ARRAY[]::text[]) @> ARRAY['EDITOR']::text[] THEN TRUE
                                    ELSE FALSE
                                END AS has_admin_scope
                            FROM public.person_invites pi
                            INNER JOIN public.organisations oi_org
                                ON oi_org.id = pi.organisation_id
                            INNER JOIN user_management.organisations um_org
                                ON um_org.cdp_organisation_guid = oi_org.guid
                            WHERE pi.person_id IS NULL
                              AND (pi.expires_on IS NULL OR pi.expires_on >= NOW())
                              AND oi_org.roles && ARRAY[1, 3, 4]::integer[]
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
                            qi.organisation_id,
                            app.id,
                            TRUE,
                            NOW(),
                            'migration:backfill-invite-roles',
                            FALSE,
                            NOW(),
                            'migration:backfill-invite-roles'
                        FROM qualifying_invites qi
                        CROSS JOIN user_management.applications app
                        WHERE app.client_id = 'find-a-tender'
                        ON CONFLICT (organisation_id, application_id) DO UPDATE
                        SET
                            is_active = TRUE,
                            enabled_at = NOW(),
                            enabled_by = 'migration:backfill-invite-roles',
                            disabled_at = NULL,
                            disabled_by = NULL,
                            is_deleted = FALSE,
                            deleted_at = NULL,
                            deleted_by = NULL,
                            modified_at = NOW(),
                            modified_by = 'migration:backfill-invite-roles';

                        WITH qualifying_invites AS (
                            SELECT DISTINCT
                                pi.guid AS cdp_person_invite_guid,
                                um_org.id AS organisation_id,
                                CASE
                                    WHEN oi_org.roles @> ARRAY[1]::integer[] THEN TRUE
                                    ELSE FALSE
                                END AS has_buyer_role,
                                CASE
                                    WHEN oi_org.roles && ARRAY[3, 4]::integer[] THEN TRUE
                                    ELSE FALSE
                                END AS has_supplier_or_tenderer_role,
                                CASE
                                    WHEN COALESCE(pi.scopes, ARRAY[]::text[]) @> ARRAY['ADMIN']::text[] THEN TRUE
                                    ELSE FALSE
                                END AS has_admin_scope,
                                CASE
                                    WHEN COALESCE(pi.scopes, ARRAY[]::text[]) @> ARRAY['EDITOR']::text[] THEN TRUE
                                    ELSE FALSE
                                END AS has_editor_scope
                            FROM public.person_invites pi
                            INNER JOIN public.organisations oi_org
                                ON oi_org.id = pi.organisation_id
                            INNER JOIN user_management.organisations um_org
                                ON um_org.cdp_organisation_guid = oi_org.guid
                            WHERE pi.person_id IS NULL
                              AND (pi.expires_on IS NULL OR pi.expires_on >= NOW())
                              AND oi_org.roles && ARRAY[1, 3, 4]::integer[]
                        ),
                        target_roles AS (
                            SELECT
                                qi.cdp_person_invite_guid,
                                qi.organisation_id,
                                CASE
                                    WHEN qi.has_buyer_role AND (qi.has_admin_scope OR qi.has_editor_scope) THEN 'Editor (buyer)'
                                    WHEN qi.has_supplier_or_tenderer_role AND (qi.has_admin_scope OR qi.has_editor_scope) THEN 'Editor (supplier)'
                                    WHEN qi.has_buyer_role THEN 'Viewer (buyer)'
                                    WHEN qi.has_supplier_or_tenderer_role THEN 'Viewer (supplier)'
                                    ELSE NULL
                                END AS target_role_name
                            FROM qualifying_invites qi
                        ),
                        resolved_targets AS (
                            SELECT
                                irm.id AS invite_role_mapping_id,
                                oa.id AS organisation_application_id,
                                ar.id AS application_role_id
                            FROM target_roles tr
                            INNER JOIN user_management.invite_role_mappings irm
                                ON irm.cdp_person_invite_guid = tr.cdp_person_invite_guid
                               AND irm.is_deleted = FALSE
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
                        DELETE FROM user_management.invite_role_application_assignments iraa
                        USING resolved_targets rt,
                              user_management.organisation_applications oa,
                              user_management.applications app,
                              user_management.application_roles ar
                        WHERE iraa.invite_role_mapping_id = rt.invite_role_mapping_id
                          AND iraa.organisation_application_id = oa.id
                          AND oa.application_id = app.id
                          AND app.client_id = 'find-a-tender'
                          AND iraa.application_role_id = ar.id
                          AND iraa.application_role_id <> rt.application_role_id;

                        WITH qualifying_invites AS (
                            SELECT DISTINCT
                                pi.guid AS cdp_person_invite_guid,
                                um_org.id AS organisation_id,
                                CASE
                                    WHEN oi_org.roles @> ARRAY[1]::integer[] THEN TRUE
                                    ELSE FALSE
                                END AS has_buyer_role,
                                CASE
                                    WHEN oi_org.roles && ARRAY[3, 4]::integer[] THEN TRUE
                                    ELSE FALSE
                                END AS has_supplier_or_tenderer_role,
                                CASE
                                    WHEN COALESCE(pi.scopes, ARRAY[]::text[]) @> ARRAY['ADMIN']::text[] THEN TRUE
                                    ELSE FALSE
                                END AS has_admin_scope,
                                CASE
                                    WHEN COALESCE(pi.scopes, ARRAY[]::text[]) @> ARRAY['EDITOR']::text[] THEN TRUE
                                    ELSE FALSE
                                END AS has_editor_scope
                            FROM public.person_invites pi
                            INNER JOIN public.organisations oi_org
                                ON oi_org.id = pi.organisation_id
                            INNER JOIN user_management.organisations um_org
                                ON um_org.cdp_organisation_guid = oi_org.guid
                            WHERE pi.person_id IS NULL
                              AND (pi.expires_on IS NULL OR pi.expires_on >= NOW())
                              AND oi_org.roles && ARRAY[1, 3, 4]::integer[]
                        ),
                        target_roles AS (
                            SELECT
                                qi.cdp_person_invite_guid,
                                qi.organisation_id,
                                CASE
                                    WHEN qi.has_buyer_role AND (qi.has_admin_scope OR qi.has_editor_scope) THEN 'Editor (buyer)'
                                    WHEN qi.has_supplier_or_tenderer_role AND (qi.has_admin_scope OR qi.has_editor_scope) THEN 'Editor (supplier)'
                                    WHEN qi.has_buyer_role THEN 'Viewer (buyer)'
                                    WHEN qi.has_supplier_or_tenderer_role THEN 'Viewer (supplier)'
                                    ELSE NULL
                                END AS target_role_name
                            FROM qualifying_invites qi
                        ),
                        resolved_targets AS (
                            SELECT
                                irm.id AS invite_role_mapping_id,
                                oa.id AS organisation_application_id,
                                ar.id AS application_role_id
                            FROM target_roles tr
                            INNER JOIN user_management.invite_role_mappings irm
                                ON irm.cdp_person_invite_guid = tr.cdp_person_invite_guid
                               AND irm.is_deleted = FALSE
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
                        INSERT INTO user_management.invite_role_application_assignments
                            (
                                invite_role_mapping_id,
                                organisation_application_id,
                                application_role_id,
                                is_deleted,
                                created_at,
                                created_by
                            )
                        SELECT DISTINCT
                            rt.invite_role_mapping_id,
                            rt.organisation_application_id,
                            rt.application_role_id,
                            FALSE,
                            NOW(),
                            'migration:backfill-invite-roles'
                        FROM resolved_targets rt
                        ON CONFLICT (invite_role_mapping_id, organisation_application_id, application_role_id) DO UPDATE
                        SET
                            is_deleted = FALSE,
                            deleted_at = NULL,
                            deleted_by = NULL,
                            modified_at = NOW(),
                            modified_by = 'migration:backfill-invite-roles';
                    END IF;
                END
                $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentional no-op: backfilled invite role data is not removed on rollback.
        }
    }
}
