using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class OrderAsmAlphabetically : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Update the DisplayOrder of form_sections based on their titles alphabetically
            migrationBuilder.Sql(@"
                WITH OrderedSections AS (
                    SELECT
                        id,
                        title,
                        ROW_NUMBER() OVER (ORDER BY title ASC) AS new_display_order
                    FROM form_sections
                    WHERE title IN (
                        'CarbonNetZero_SectionTitle',
                        'CyberEssentials_SectionTitle',
                        'DataProtection_SectionTitle',
                        'HealthAndSafety_SectionTitle',
                        'ModernSlavery_SectionTitle',
                        'Payments_SectionTitle',
                        'Steel_SectionTitle'
                    )
                )
                UPDATE form_sections
                SET display_order = os.new_display_order
                FROM OrderedSections os
                WHERE form_sections.id = os.id;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                -- Restore each section to its original display_order value
                UPDATE form_sections SET display_order = 1 WHERE title = 'DataProtection_SectionTitle';
                UPDATE form_sections SET display_order = 1 WHERE title = 'Payments_SectionTitle';
                UPDATE form_sections SET display_order = 1 WHERE title = 'Steel_SectionTitle';
                UPDATE form_sections SET display_order = 1 WHERE title = 'HealthAndSafety_SectionTitle';
                UPDATE form_sections SET display_order = 1 WHERE title = 'ModernSlavery_SectionTitle';
                UPDATE form_sections SET display_order = 1 WHERE title = 'CyberEssentials_SectionTitle';
                UPDATE form_sections SET display_order = 1 WHERE title = 'CarbonNetZero_SectionTitle';
            ");
        }
    }
}
