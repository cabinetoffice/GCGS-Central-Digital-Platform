using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FinancialInformationLocalization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                DO $$
                BEGIN
                    UPDATE form_questions
                    SET title = 'FinancialInformation_01_Title',
                    summary_title = 'FinancialInformation_01_SummaryTitle'
                    WHERE name = '_FinancialInformation01';

                    UPDATE form_questions
                    SET title = 'FinancialInformation_02_Title',
                    description = 'FinancialInformation_02_Description',
                    summary_title = 'FinancialInformation_02_SummaryTitle'
                    WHERE name = '_FinancialInformation02';

                    UPDATE form_questions
                    SET title = 'FinancialInformation_03_Title',
                    summary_title = 'FinancialInformation_03_SummaryTitle'
                    WHERE name = '_FinancialInformation03';

                    UPDATE form_questions
                    SET title = 'FinancialInformation_04_Title',
                    description = 'FinancialInformation_04_Description'
                    WHERE name = '_FinancialInformation04';

                    UPDATE form_questions
                    SET title = 'Global_CheckYourAnswers'
                    WHERE name = '_FinancialInformation05';

                    UPDATE form_sections
                    SET configuration = '{{""AddAnotherAnswerLabel"": ""FinancialInformation_Configuration_AddAnotherAnswerLabel"", ""SingularSummaryHeading"": ""FinancialInformation_Configuration_SingularSummaryHeading"", ""RemoveConfirmationCaption"": ""FinancialInformation_SectionTitle"", ""RemoveConfirmationHeading"": ""FinancialInformation_Configuration_RemoveConfirmationHeading"", ""PluralSummaryHeadingFormat"": ""FinancialInformation_Configuration_PluralSummaryHeadingFormat"", ""FurtherQuestionsExemptedHint"": ""FinancialInformation_Configuration_FurtherQuestionsExemptedHint"", ""FurtherQuestionsExemptedHeading"": ""FinancialInformation_Configuration_FurtherQuestionsExemptedHeading""}}'
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
