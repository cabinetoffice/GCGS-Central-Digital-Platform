using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWelshHealthAndSafetyQuestion02Description : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            {
                migrationBuilder.Sql(@"
                UPDATE form_questions
                SET description = 'WelshHealthAndSafetyQuestion_02_Description'
                WHERE title = 'WelshHealthAndSafetyQuestion_02_Title';
            ");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE form_questions
                SET description = NULL
                WHERE title = 'WelshHealthAndSafetyQuestion_02_Title';
            ");
        }
    }
}
