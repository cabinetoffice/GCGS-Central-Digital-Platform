using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.UserManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProcurementApplications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                INSERT INTO user_management.applications
                    (name, client_id, description, category, is_active, is_deleted, created_at, created_by)
                VALUES
                    ('Supplier Information and Registration Service', 'supplier-information-and-registration-service', 'Register your organisation, complete supplier information, and share it with contracting authorities when bidding for public contracts.', 'Procurement', TRUE, FALSE, NOW(), 'migration:add-procurement-applications'),
                    ('Find a Tender', 'find-a-tender', 'Publish and search for procurement notices for high-value UK public contracts.', 'Procurement', TRUE, FALSE, NOW(), 'migration:add-procurement-applications'),
                    ('Contracts Finder', 'contracts-finder', 'Publish and search for procurement notices for low-value UK public contracts.', 'Procurement', TRUE, FALSE, NOW(), 'migration:add-procurement-applications'),
                    ('AI SoW Tool', 'ai-sow-tool', 'Use AI to draft and review statements of work against procurement quality standards and requirements.', 'Procurement', TRUE, FALSE, NOW(), 'migration:add-procurement-applications'),
                    ('Financial Viability Risk Assessments', 'financial-viability-risk-assessments', 'Set up and manage financial viability assessments for suppliers, or complete and submit them in response to a contracting authority.', 'Procurement', TRUE, FALSE, NOW(), 'migration:add-procurement-applications')
                ON CONFLICT (client_id) DO NOTHING;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM user_management.applications
                WHERE created_by = 'migration:add-procurement-applications'
                  AND client_id IN
                  (
                      'supplier-information-and-registration-service',
                      'find-a-tender',
                      'contracts-finder',
                      'ai-sow-tool',
                      'financial-viability-risk-assessments'
                  );
                """);
        }
    }
}
