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
                    configuration = '{{""AddAnotherAnswerLabel"": ""Add another exclusion?"", ""SingularSummaryHeading"": ""You have added 1 exclusion"", ""RemoveConfirmationCaption"": ""Exclusions"", ""RemoveConfirmationHeading"": ""Are you sure you want to remove this exclusion?"", ""PluralSummaryHeadingFormat"": ""You have added {{0}} exclusions"", ""FurtherQuestionsExemptedHeading"": ""<legend class=''govuk-fieldset__legend govuk-fieldset__legend--l''><h1 class=''govuk-fieldset__heading''>Do you have any exclusions to add for your organisation or a connected person?</h1></legend>""}}'
                WHERE guid = '8a75cb04-fe29-45ae-90f9-168832dbea48';
            ");

            migrationBuilder.Sql($@"
                UPDATE form_sections
                SET
                    check_further_questions_exempted = TRUE,
                    configuration = '{{""AddAnotherAnswerLabel"": ""Add another qualification?"", ""SingularSummaryHeading"": ""You have added 1 qualification"", ""RemoveConfirmationCaption"": ""Qualifications"", ""RemoveConfirmationHeading"": ""Are you sure you want to remove this qualification?"", ""PluralSummaryHeadingFormat"": ""You have added {{0}} qualifications"", ""FurtherQuestionsExemptedHeading"": ""<legend class=''govuk-fieldset__legend govuk-fieldset__legend--l''><h1 class=''govuk-fieldset__heading''>Do you want to add any relevant qualifications?</h1></legend><div class=''govuk-hint''>These are general qualifications for business and trade, not procurement specific. Adding them will provide evidence of your suitability to contracting authorities and support any bids you submit. For example, ISO certification.</div>""}}'
                WHERE guid = '798cf1c1-40be-4e49-9adb-252219d5599d';
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
