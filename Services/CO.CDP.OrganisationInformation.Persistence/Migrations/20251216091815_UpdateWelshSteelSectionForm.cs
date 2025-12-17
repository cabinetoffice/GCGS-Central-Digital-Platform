using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateWelshSteelSectionForm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = $@"
                DO $$
                BEGIN
                    UPDATE form_questions SET caption = 'WelshSteel_06_Caption' WHERE title = 'WelshSteel_06_Title';
                    UPDATE form_questions SET caption = 'WelshSteel_07_Caption' WHERE title = 'WelshSteel_07_Title';
                    UPDATE form_questions SET caption = 'WelshSteel_08_Caption' WHERE title = 'WelshSteel_08_Title';
                END $$;
            ";

            migrationBuilder.Sql(sql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var sql = $@"
                DO $$
                BEGIN
                    UPDATE form_questions SET caption = NULL WHERE title = 'WelshSteel_06_Title';
                    UPDATE form_questions SET caption = NULL WHERE title = 'WelshSteel_07_Title';
                    UPDATE form_questions SET caption = NULL WHERE title = 'WelshSteel_08_Title';
                END $$;
            ";

            migrationBuilder.Sql(sql);
        }
    }
}
