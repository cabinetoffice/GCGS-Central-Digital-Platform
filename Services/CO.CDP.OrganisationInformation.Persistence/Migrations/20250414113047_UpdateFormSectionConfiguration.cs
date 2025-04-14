using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFormSectionConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                DO $$
                BEGIN                    

                    UPDATE form_sections
                    SET configuration = '{{""AddAnotherAnswerLabel"": ""Exclusions_Configuration_AddAnotherAnswerLabel"", ""SingularSummaryHeading"": ""Exclusions_Configuration_SingularSummaryHeading"", ""SingularSummaryHeadingHint"": ""Exclusions_Configuration_SingularSummaryHeadingHint"", ""RemoveConfirmationCaption"": ""Exclusions_SectionTitle"", ""RemoveConfirmationHeading"": ""Exclusions_Configuration_RemoveConfirmationHeading"", ""PluralSummaryHeadingFormat"": ""Exclusions_Configuration_PluralSummaryHeadingFormat"", ""PluralSummaryHeadingHintFormat"": ""Exclusions_Configuration_PluralSummaryHeadingHintFormat"", ""FurtherQuestionsExemptedHeading"": ""Exclusions_Configuration_FurtherQuestionsExemptedHeading""}}'
                    WHERE title = 'Exclusions_SectionTitle';

                    UPDATE form_sections
                    SET configuration = '{{""AddAnotherAnswerLabel"": ""Qualifications_Configuration_AddAnotherAnswerLabel"", ""SingularSummaryHeading"": ""Qualifications_Configuration_SingularSummaryHeading"", ""SingularSummaryHeadingHint"": ""Qualifications_Configuration_SingularSummaryHeadingHint"", ""RemoveConfirmationCaption"": ""Qualifications_SectionTitle"", ""RemoveConfirmationHeading"": ""Qualifications_Configuration_RemoveConfirmationHeading"", ""PluralSummaryHeadingFormat"": ""Qualifications_Configuration_PluralSummaryHeadingFormat"", ""PluralSummaryHeadingHintFormat"": ""Qualifications_Configuration_PluralSummaryHeadingHintFormat"", ""FurtherQuestionsExemptedHint"": ""Qualifications_Configuration_FurtherQuestionsExemptedHint"", ""FurtherQuestionsExemptedHeading"": ""Qualifications_Configuration_FurtherQuestionsExemptedHeading""}}'
                    WHERE title = 'Qualifications_SectionTitle';

                    UPDATE form_sections
                    SET configuration = '{{""AddAnotherAnswerLabel"": ""TradeAssurances_Configuration_AddAnotherAnswerLabel"", ""SingularSummaryHeading"": ""TradeAssurances_Configuration_SingularSummaryHeading"", ""SingularSummaryHeadingHint"": ""TradeAssurances_Configuration_SingularSummaryHeadingHint"", ""RemoveConfirmationCaption"": ""TradeAssurances_SectionTitle"", ""RemoveConfirmationHeading"": ""TradeAssurances_Configuration_RemoveConfirmationHeading"", ""PluralSummaryHeadingFormat"": ""TradeAssurances_Configuration_PluralSummaryHeadingFormat"", ""PluralSummaryHeadingHintFormat"": ""TradeAssurances_Configuration_PluralSummaryHeadingHintFormat"", ""FurtherQuestionsExemptedHint"": ""TradeAssurances_Configuration_FurtherQuestionsExemptedHint"", ""FurtherQuestionsExemptedHeading"": ""TradeAssurances_Configuration_FurtherQuestionsExemptedHeading""}}'
                    WHERE title = 'TradeAssurances_SectionTitle';

                    UPDATE form_sections
                    SET configuration = '{{""AddAnotherAnswerLabel"": ""FinancialInformation_Configuration_AddAnotherAnswerLabel"", ""SingularSummaryHeading"": ""FinancialInformation_Configuration_SingularSummaryHeading"", ""SingularSummaryHeadingHint"": ""FinancialInformation_Configuration_SingularSummaryHeadingHint"", ""RemoveConfirmationCaption"": ""FinancialInformation_SectionTitle"", ""RemoveConfirmationHeading"": ""FinancialInformation_Configuration_RemoveConfirmationHeading"", ""PluralSummaryHeadingFormat"": ""FinancialInformation_Configuration_PluralSummaryHeadingFormat"", ""PluralSummaryHeadingHintFormat"": ""FinancialInformation_Configuration_PluralSummaryHeadingHintFormat"", ""FurtherQuestionsExemptedHint"": ""FinancialInformation_Configuration_FurtherQuestionsExemptedHint"", ""FurtherQuestionsExemptedHeading"": ""FinancialInformation_Configuration_FurtherQuestionsExemptedHeading""}}'
                    WHERE title = 'FinancialInformation_SectionTitle';

                END $$;
             ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
