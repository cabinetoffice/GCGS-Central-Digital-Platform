using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    public partial class UpdateHealthAndSafety_03 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE form_questions
                SET options = '{""layout"": {""button"": {""beforeButtonContent"": ""HealthAndSafetyQuestion_03_CustomInsetText""}}}'::jsonb
                WHERE title = 'HealthAndSafetyQuestion_03_Title';
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE form_questions
                SET options = '{}'::jsonb
                WHERE title = 'HealthAndSafetyQuestion_03_Title';
            ");
        }
    }
}