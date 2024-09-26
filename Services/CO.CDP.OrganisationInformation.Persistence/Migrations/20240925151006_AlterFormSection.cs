using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AlterFormSection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "check_further_questions_exempted",
                table: "form_sections",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql($@"
                UPDATE form_sections
                SET
                    check_further_questions_exempted = TRUE,
                    configuration = '{{""AddAnotherAnswerLabel"": ""Add another exclusion?"", ""SingularSummaryHeading"": ""You have added 1 exclusions"", ""RemoveConfirmationCaption"": ""Exclusions"", ""RemoveConfirmationHeading"": ""Are you sure you want to remove this exclusion?"", ""PluralSummaryHeadingFormat"": ""You have added {{0}} exclusions"", ""FurtherQuestionsExemptedHeading"": ""<h1 class=''govuk-heading-l''>Do you have any exclusions to add for your organisation or a connected person?</h1>""}}'
                WHERE guid = '8a75cb04-fe29-45ae-90f9-168832dbea48';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "check_further_questions_exempted",
                table: "form_sections");
        }
    }
}
