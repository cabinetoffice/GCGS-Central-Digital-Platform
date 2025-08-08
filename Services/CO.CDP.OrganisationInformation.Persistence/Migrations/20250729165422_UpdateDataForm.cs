using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDataForm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM form_questions WHERE title = 'DataProtection_02_Title') THEN
                        UPDATE form_questions
                        SET options = '{""layout"": {""customYesText"": ""DataProtection_02_Custom_Yes"", ""customNoText"": ""DataProtection_02_Custom_No""}}'
                        WHERE title = 'DataProtection_02_Title';
                    END IF;

                    IF EXISTS (SELECT 1 FROM form_questions WHERE title = 'DataProtection_03_Title') THEN
                        UPDATE form_questions
                        SET options = '{""layout"": {""customYesText"": ""DataProtection_03_Custom_Yes"", ""customNoText"": ""DataProtection_03_Custom_No""}}'
                        WHERE title = 'DataProtection_03_Title';
                    END IF;
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
                    SET options = '{}'
                    WHERE title IN ('DataProtection_02_Title', 'DataProtection_03_Title');
                END $$;
            ");
        }
    }
}
