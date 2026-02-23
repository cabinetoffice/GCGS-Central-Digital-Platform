using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.UserManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class KeepPaymentsAndFvraApplications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                INSERT INTO user_management.applications
                    (name, client_id, description, category, is_active, is_deleted, created_at, created_by)
                VALUES
                    ('Payments', 'payments', 'Manage payment information and approvals across procurement activities.', 'Procurement', TRUE, FALSE, NOW(), 'migration:keep-payments-fvra'),
                    ('Financial Viability Risk Assessments', 'financial-viability-risk-assessments', 'Set up and manage financial viability assessments for suppliers, or complete and submit them in response to a contracting authority.', 'Procurement', TRUE, FALSE, NOW(), 'migration:keep-payments-fvra')
                ON CONFLICT (client_id) DO UPDATE
                SET
                    name = EXCLUDED.name,
                    description = EXCLUDED.description,
                    category = EXCLUDED.category,
                    is_active = TRUE,
                    is_deleted = FALSE,
                    deleted_at = NULL,
                    deleted_by = NULL,
                    modified_at = NOW(),
                    modified_by = 'migration:keep-payments-fvra';
                """);

            migrationBuilder.Sql(
                """
                DELETE FROM user_management.applications
                WHERE client_id NOT IN ('payments', 'financial-viability-risk-assessments');
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO user_management.organisation_applications
                    (organisation_id, application_id, is_active, enabled_at, enabled_by, is_deleted, created_at, created_by)
                SELECT
                    org.id,
                    app.id,
                    TRUE,
                    NOW(),
                    'migration:keep-payments-fvra',
                    FALSE,
                    NOW(),
                    'migration:keep-payments-fvra'
                FROM user_management.organisations org
                CROSS JOIN user_management.applications app
                WHERE app.client_id IN ('payments', 'financial-viability-risk-assessments')
                ON CONFLICT (organisation_id, application_id) DO UPDATE
                SET
                    is_active = TRUE,
                    enabled_at = NOW(),
                    enabled_by = 'migration:keep-payments-fvra',
                    is_deleted = FALSE,
                    deleted_at = NULL,
                    deleted_by = NULL,
                    modified_at = NOW(),
                    modified_by = 'migration:keep-payments-fvra';
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO user_management.application_roles
                    (application_id, name, description, is_active, is_deleted, created_at, created_by)
                SELECT app.id, role_data.name, role_data.description, TRUE, FALSE, NOW(), 'migration:keep-payments-fvra'
                FROM user_management.applications app
                INNER JOIN (
                    VALUES
                        ('payments', 'Admin', 'Full access to view and edit payment information on all pages.'),
                        ('payments', 'Editor', 'Full access to edit existing payments.'),
                        ('payments', 'Authoriser', 'Full access to authorise or approve payments.'),
                        ('financial-viability-risk-assessments', 'Assessor (internal)', 'Can review submitted financial viability evidence for suppliers within your authority and record assessment outcomes.'),
                        ('financial-viability-risk-assessments', 'Assessor (external)', 'Can review submitted financial viability evidence for assigned suppliers as an external assessor and record assessment outcomes.'),
                        ('financial-viability-risk-assessments', 'QA (internal)', 'Can quality-assure completed assessments, request amendments, and sign off internal assessment recommendations.'),
                        ('financial-viability-risk-assessments', 'QA (external)', 'Can quality-assure completed assessments for assigned cases and provide external QA outcomes and feedback.'),
                        ('financial-viability-risk-assessments', 'Author and Collaborator (internal)', 'Can create, edit, and submit FVRA responses and collaborate with internal colleagues on evidence.'),
                        ('financial-viability-risk-assessments', 'Author and Collaborator (external)', 'Can create, edit, and submit FVRA responses on behalf of your organisation as an external collaborator with delegated access.')
                ) AS role_data(client_id, name, description)
                    ON role_data.client_id = app.client_id
                ON CONFLICT (application_id, name) DO UPDATE
                SET
                    description = EXCLUDED.description,
                    is_active = TRUE,
                    is_deleted = FALSE,
                    deleted_at = NULL,
                    deleted_by = NULL,
                    modified_at = NOW(),
                    modified_by = 'migration:keep-payments-fvra';
                """);

            migrationBuilder.Sql(
                """
                DELETE FROM user_management.application_roles ar
                USING user_management.applications app
                WHERE ar.application_id = app.id
                  AND (
                    (app.client_id = 'payments'
                     AND ar.name NOT IN ('Admin', 'Editor', 'Authoriser'))
                    OR
                    (app.client_id = 'financial-viability-risk-assessments'
                     AND ar.name NOT IN (
                        'Assessor (internal)',
                        'Assessor (external)',
                        'QA (internal)',
                        'QA (external)',
                        'Author and Collaborator (internal)',
                        'Author and Collaborator (external)'
                     ))
                  );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM user_management.application_roles
                WHERE (created_by = 'migration:keep-payments-fvra' OR modified_by = 'migration:keep-payments-fvra')
                  AND name IN (
                    'Admin',
                    'Editor',
                    'Authoriser',
                    'Assessor (internal)',
                    'Assessor (external)',
                    'QA (internal)',
                    'QA (external)',
                    'Author and Collaborator (internal)',
                    'Author and Collaborator (external)'
                  );
                """);

            migrationBuilder.Sql(
                """
                DELETE FROM user_management.applications
                WHERE created_by = 'migration:keep-payments-fvra'
                  AND client_id = 'payments';
                """);

            migrationBuilder.Sql(
                """
                DELETE FROM user_management.organisation_applications oa
                USING user_management.applications app
                WHERE oa.application_id = app.id
                  AND oa.created_by = 'migration:keep-payments-fvra'
                  AND app.client_id IN ('payments', 'financial-viability-risk-assessments');
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO user_management.applications
                    (name, client_id, description, category, is_active, is_deleted, created_at, created_by)
                VALUES
                    ('Supplier Information and Registration Service', 'supplier-information-and-registration-service', 'Register your organisation, complete supplier information, and share it with contracting authorities when bidding for public contracts.', 'Procurement', TRUE, FALSE, NOW(), 'migration:add-procurement-applications'),
                    ('Find a Tender', 'find-a-tender', 'Publish and search for procurement notices for high-value UK public contracts.', 'Procurement', TRUE, FALSE, NOW(), 'migration:add-procurement-applications'),
                    ('Contracts Finder', 'contracts-finder', 'Publish and search for procurement notices for low-value UK public contracts.', 'Procurement', TRUE, FALSE, NOW(), 'migration:add-procurement-applications'),
                    ('AI SoW Tool', 'ai-sow-tool', 'Use AI to draft and review statements of work against procurement quality standards and requirements.', 'Procurement', TRUE, FALSE, NOW(), 'migration:add-procurement-applications')
                ON CONFLICT (client_id) DO NOTHING;
                """);
        }
    }
}
