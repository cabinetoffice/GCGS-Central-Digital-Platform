using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TradeAssurancesLocalization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                DO $$
                BEGIN
                    UPDATE form_questions
                    SET description = 'TradeAssurance_01_Description',
                    title = 'TradeAssurance_01_Title',
                    summary_title = 'TradeAssurance_01_SummaryTitle'
                    WHERE name = '_TradeAssurance01';

                    UPDATE form_questions
                    SET title = 'TradeAssurance_02_Title',
                    caption = 'TradeAssurance_02_Caption',
                    summary_title = 'TradeAssurance_02_SummaryTitle'
                    WHERE name = '_TradeAssurance02';

                    UPDATE form_questions
                    SET title = 'TradeAssurance_03_Title',
                    summary_title = 'TradeAssurance_03_SummaryTitle'
                    WHERE name = '_TradeAssurance03';

                    UPDATE form_questions
                    SET title = 'Global_CheckYourAnswers'
                    WHERE name = '_TradeAssurance04';

                    UPDATE form_sections
                    SET configuration = '{{""AddAnotherAnswerLabel"": ""TradeAssurances_Configuration_AddAnotherAnswerLabel"", ""SingularSummaryHeading"": ""TradeAssurances_Configuration_SingularSummaryHeading"", ""RemoveConfirmationCaption"": ""TradeAssurances_SectionTitle"", ""RemoveConfirmationHeading"": ""TradeAssurances_Configuration_RemoveConfirmationHeading"", ""PluralSummaryHeadingFormat"": ""TradeAssurances_Configuration_PluralSummaryHeadingFormat"", ""FurtherQuestionsExemptedHint"": ""TradeAssurances_Configuration_FurtherQuestionsExemptedHint"", ""FurtherQuestionsExemptedHeading"": ""TradeAssurances_Configuration_FurtherQuestionsExemptedHeading""}}'
                    WHERE title = 'TradeAssurances_SectionTitle';
                END $$;
             ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
