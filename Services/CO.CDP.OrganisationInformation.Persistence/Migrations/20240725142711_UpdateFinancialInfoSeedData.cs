using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFinancialInfoSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                UPDATE form_sections
                SET configuration = '{{""AddAnotherAnswerLabel"": ""Add another file?"", ""SingularSummaryHeading"": ""You have added 1 file"", ""RemoveConfirmationCaption"": ""Economic and Financial Standing"", ""RemoveConfirmationHeading"": ""Are you sure you want to remove this file?"", ""PluralSummaryHeadingFormat"": ""You have added {{0}} files""}}'
                WHERE guid = '13511cb1-9ed4-4d72-ba9e-05b4a0be880c';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                UPDATE form_sections
                SET configuration = '{{}}'
                WHERE guid = '13511cb1-9ed4-4d72-ba9e-05b4a0be880c';
            ");
        }
    }
}
