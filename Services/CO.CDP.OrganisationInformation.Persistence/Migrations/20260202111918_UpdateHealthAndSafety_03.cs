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
                SET options = '{""layout"":{""button"":{""beforeButtonContent"":""HealthAndSafetyQuestion_02_CustomInsetText""}}}'::jsonb
                WHERE title = 'HealthAndSafetyQuestion_02_Title';
            ");


            migrationBuilder.Sql(@"
                UPDATE form_questions
                SET description = 'HealthAndSafetyQuestion_03_Description'
                WHERE title = 'HealthAndSafetyQuestion_03_Title';
            ");

            // Q03: Remove previously-set beforeButtonContent
            migrationBuilder.Sql(@"
                UPDATE form_questions
                SET options = COALESCE(options, '{}'::jsonb) #- '{layout,button,beforeButtonContent}'
                WHERE title = 'HealthAndSafetyQuestion_03_Title';
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE form_questions
                SET description = NULL
                WHERE title = 'HealthAndSafetyQuestion_03_Title';
            ");


            migrationBuilder.Sql(@"
                UPDATE form_questions
                SET options = COALESCE(options, '{}'::jsonb) #- '{layout,button,beforeButtonContent}'
                WHERE title IN ('HealthAndSafetyQuestion_02_Title', 'HealthAndSafetyQuestion_03_Title');
            ");
        }
    }
}