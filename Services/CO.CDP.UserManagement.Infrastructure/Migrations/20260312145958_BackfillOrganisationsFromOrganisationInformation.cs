using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.UserManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BackfillOrganisationsFromOrganisationInformation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Backfill organisations from public.organisations (OrganisationInformation) into user_management.organisations.
            //
            // OrganisationInformationContext does not set a default schema, so its tables live in the public schema.
            //
            // Slug is generated from the organisation name using the same algorithm as SlugGeneratorService:
            //   1. Lowercase
            //   2. Replace whitespace runs with hyphens
            //   3. Replace any remaining non-alphanumeric/non-hyphen chars with hyphens
            //   4. Collapse multiple hyphens, trim leading/trailing hyphens
            //
            // Slug uniqueness strategy:
            //   - Source orgs that share the same base slug are disambiguated with a numeric suffix (e.g. "acme-1").
            //   - If the resulting slug already exists in user_management (from a previously synced org),
            //     the first 8 characters of the source GUID are appended as a further disambiguation suffix.
            //
            // Idempotency: orgs already present in user_management.organisations (by cdp_organisation_guid) are
            // excluded from the INSERT entirely, so the migration is safe to run multiple times.
            //
            // Guard: the DO block checks for the existence of public.organisations before
            // running the INSERT, making this migration a no-op in environments (e.g. integration test databases)
            // where the organisation_information tables are not present.
            // Note: OrganisationInformationContext uses no default schema, so its tables live in public.
            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.tables
                        WHERE table_schema = 'public'
                          AND table_name   = 'organisations'
                    ) THEN
                        WITH base_slugs AS (
                            SELECT
                                src.guid,
                                src.name,
                                trim('-' FROM
                                    regexp_replace(
                                        regexp_replace(
                                            regexp_replace(lower(src.name), '\s+', '-', 'g'),
                                            '[^a-z0-9-]', '-', 'g'
                                        ),
                                        '-+', '-', 'g'
                                    )
                                ) AS base_slug
                            FROM public.organisations src
                            WHERE NOT EXISTS (
                                SELECT 1 FROM user_management.organisations um
                                WHERE um.cdp_organisation_guid = src.guid
                            )
                        ),
                        ranked AS (
                            SELECT
                                guid,
                                name,
                                base_slug,
                                ROW_NUMBER() OVER (PARTITION BY base_slug ORDER BY guid) AS rn
                            FROM base_slugs
                        ),
                        with_candidate_slug AS (
                            SELECT
                                guid,
                                name,
                                CASE
                                    WHEN rn = 1 THEN base_slug
                                    ELSE base_slug || '-' || (rn - 1)::text
                                END AS candidate_slug
                            FROM ranked
                        )
                        INSERT INTO user_management.organisations
                            (cdp_organisation_guid, name, slug, is_active, is_deleted, created_at, created_by)
                        SELECT
                            wcs.guid,
                            wcs.name,
                            CASE
                                WHEN EXISTS (
                                    SELECT 1 FROM user_management.organisations um
                                    WHERE um.slug = wcs.candidate_slug
                                )
                                THEN wcs.candidate_slug || '-' || LEFT(wcs.guid::text, 8)
                                ELSE wcs.candidate_slug
                            END AS slug,
                            TRUE,
                            FALSE,
                            NOW(),
                            'migration:backfill-organisations'
                        FROM with_candidate_slug wcs;
                    END IF;
                END
                $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentional no-op: backfilled organisations are not removed on rollback.
        }
    }
}
