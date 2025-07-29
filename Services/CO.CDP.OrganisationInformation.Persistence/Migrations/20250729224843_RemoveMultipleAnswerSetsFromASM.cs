using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMultipleAnswerSetsFromASM : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE form_sections
                SET ""configuration"" = '{}',
                    ""allows_multiple_answer_sets"" = false,
                    ""check_further_questions_exempted"" = false
                WHERE ""title"" IN (
                    'Steel_SectionTitle',
                    'HealthAndSafety_SectionTitle',
                    'ModernSlavery_SectionTitle',
                    'CyberEssentials_SectionTitle',
                    'DataProtection_SectionTitle',
                    'CarbonNetZero_SectionTitle',
                    'Payments_SectionTitle'
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // fixing broken data, not needed
        }
    }
}
