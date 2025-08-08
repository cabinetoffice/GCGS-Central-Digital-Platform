using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCaptionAndStartButtons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    UPDATE form_questions
                    SET caption = 'CarbonNetZero_03_Caption'
                    WHERE title = 'CarbonNetZero_03_Title';

                    UPDATE form_questions
                    SET ""options"" = '{""layout"": {""button"": {""style"": ""Start"", ""text"": ""Global_Start""}}}'::jsonb
                    WHERE title IN (
                        'DataProtection_01_Title',
                        'CyberEssentials_01_Title',
                        'CarbonNetZero_01_Title',
                        'ModernSlavery_01_Title',
                        'HealthAndSafetyQuestion_01_Title',
                        'SteelQuestion_01_Title'
                    );
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$
                BEGIN

                    UPDATE form_questions
                    SET caption = NULL
                    WHERE title = 'CarbonNetZero_03_Title';

                    UPDATE form_questions
                    SET ""options"" = '{}'::jsonb
                    WHERE title IN (
                        'DataProtection_01_Title',
                        'CyberEssentials_01_Title',
                        'CarbonNetZero_01_Title',
                        'ModernSlavery_01_Title',
                        'HealthAndSafetyQuestion_01_Title',
                        'SteelQuestion_01_Title'
                    );
                END $$;
            ");
        }
    }
}
