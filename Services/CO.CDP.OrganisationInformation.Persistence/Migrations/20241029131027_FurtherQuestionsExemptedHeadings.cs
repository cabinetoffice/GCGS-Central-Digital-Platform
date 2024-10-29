using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FurtherQuestionsExemptedHeadings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var json = "{\"AddAnotherAnswerLabel\": \"Add another exclusion?\", \"SingularSummaryHeading\": \"You have added 1 exclusion\", \"RemoveConfirmationCaption\": \"Exclusions\", \"RemoveConfirmationHeading\": \"Are you sure you want to remove this exclusion?\", \"PluralSummaryHeadingFormat\": \"You have added {0} exclusions\", \"FurtherQuestionsExemptedHeading\": \"Do you have any exclusions to add for your organisation or a connected person?\"}";
            migrationBuilder.Sql($"UPDATE form_sections SET configuration='{json}' WHERE title = 'Exclusions_SectionTitle';");

            json = "{\"AddAnotherAnswerLabel\": \"Add another qualification?\", \"SingularSummaryHeading\": \"You have added 1 qualification\", \"RemoveConfirmationCaption\": \"Qualifications\", \"RemoveConfirmationHeading\": \"Are you sure you want to remove this qualification?\", \"PluralSummaryHeadingFormat\": \"You have added {0} qualifications\", \"FurtherQuestionsExemptedHeading\": \"Do you want to add any relevant qualifications?\", \"FurtherQuestionsExemptedHint\": \"These are general qualifications for business and trade, not procurement specific. Adding them will provide evidence of your suitability to contracting authorities and support any bids you submit. For example, ISO certification.\"}";
            migrationBuilder.Sql($"UPDATE form_sections SET configuration='{json}' WHERE title = 'Qualifications_SectionTitle';");

            json = "{\"AddAnotherAnswerLabel\": \"Add another trade assurance?\", \"SingularSummaryHeading\": \"You have added 1 trade assurance\", \"RemoveConfirmationCaption\": \"Trade assurance\", \"RemoveConfirmationHeading\": \"Are you sure you want to remove this trade assurance?\", \"PluralSummaryHeadingFormat\": \"You have added {0} trade assurances\", \"FurtherQuestionsExemptedHeading\": \"Do you want to add any trade assurances?\", \"FurtherQuestionsExemptedHint\": \"These are trade assurances for business and trade, not procurement specific. Adding them will provide confidence to contracting authorities and support any bids you submit. For example, Red Tractor Assurance covers food safety, traceability, animal welfare and environmental protection.\"}";
            migrationBuilder.Sql($"UPDATE form_sections SET configuration='{json}' WHERE title = 'TradeAssurances_SectionTitle';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var json = "{\"AddAnotherAnswerLabel\": \"Add another exclusion?\", \"SingularSummaryHeading\": \"You have added 1 exclusion\", \"RemoveConfirmationCaption\": \"Exclusions\", \"RemoveConfirmationHeading\": \"Are you sure you want to remove this exclusion?\", \"PluralSummaryHeadingFormat\": \"You have added {0} exclusions\", \"FurtherQuestionsExemptedHeading\": \"<legend class='govuk-fieldset__legend govuk-fieldset__legend--l'><h1 class='govuk-fieldset__heading'>Do you have any exclusions to add for your organisation or a connected person?</h1></legend>\"}";
            migrationBuilder.Sql($"UPDATE form_sections SET configuration='{json}' WHERE title = 'Exclusions_SectionTitle';");

            json = "{\"AddAnotherAnswerLabel\": \"Add another qualification?\", \"SingularSummaryHeading\": \"You have added 1 qualification\", \"RemoveConfirmationCaption\": \"Qualifications\", \"RemoveConfirmationHeading\": \"Are you sure you want to remove this qualification?\", \"PluralSummaryHeadingFormat\": \"You have added {0} qualifications\", \"FurtherQuestionsExemptedHeading\": \"<legend class='govuk-fieldset__legend govuk-fieldset__legend--l'><h1 class='govuk-fieldset__heading'>Do you want to add any relevant qualifications?</h1></legend><div class='govuk-hint'>These are general qualifications for business and trade, not procurement specific. Adding them will provide evidence of your suitability to contracting authorities and support any bids you submit. For example, ISO certification.</div>\"}";
            migrationBuilder.Sql($"UPDATE form_sections SET configuration='{json}' WHERE title = 'Qualifications_SectionTitle';");

            json = "{\"AddAnotherAnswerLabel\": \"Add another trade assurance?\", \"SingularSummaryHeading\": \"You have added 1 trade assurance\", \"RemoveConfirmationCaption\": \"Trade assurance\", \"RemoveConfirmationHeading\": \"Are you sure you want to remove this trade assurance?\", \"PluralSummaryHeadingFormat\": \"You have added {0} trade assurances\", \"FurtherQuestionsExemptedHeading\": \"<legend class='govuk-fieldset__legend govuk-fieldset__legend--l'><h1 class='govuk-fieldset__heading'>Do you want to add any trade assurances?</h1></legend><div class='govuk-hint'>These are trade assurances for business and trade, not procurement specific. Adding them will provide confidence to contracting authorities and support any bids you submit. For example, Red Tractor Assurance covers food safety, traceability, animal welfare and environmental protection.</div>\"}";
            migrationBuilder.Sql($"UPDATE form_sections SET configuration='{json}' WHERE title = 'TradeAssurances_SectionTitle';");
        }
    }
}
