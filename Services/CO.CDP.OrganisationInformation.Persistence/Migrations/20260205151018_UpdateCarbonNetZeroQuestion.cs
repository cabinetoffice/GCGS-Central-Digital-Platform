using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCarbonNetZeroQuestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = $@"
                DO $$
                BEGIN
                    -- update question 2 description to NULL
                    UPDATE form_questions SET description = NULL WHERE title = 'CarbonNetZero_02_Title';
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
                    -- revert question 2 description
                    UPDATE form_questions SET description = 'CarbonNetZero_02_Description' WHERE title = 'CarbonNetZero_02_Title';
                END $$;
            ";

            migrationBuilder.Sql(sql);
        }
    }
}
