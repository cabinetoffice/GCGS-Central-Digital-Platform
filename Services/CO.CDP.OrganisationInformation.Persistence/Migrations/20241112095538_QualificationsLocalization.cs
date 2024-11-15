using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class QualificationsLocalization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                DO $$
                BEGIN
                    UPDATE form_questions
                    SET description = 'Qualifications_01_Description',
                    title = 'Qualifications_01_Title',
                    summary_title = 'Qualifications_01_SummaryTitle'
                    WHERE name = '_Qualifications01';

                    UPDATE form_questions
                    SET description = 'Qualifications_02_Description',
                    title = 'Qualifications_02_Title',
                    summary_title = 'Qualifications_02_SummaryTitle'
                    WHERE name = '_Qualifications02';

                    UPDATE form_questions
                    SET title = 'Qualifications_03_Title',
                    summary_title = 'Qualifications_03_SummaryTitle'
                    WHERE name = '_Qualifications03';

                    UPDATE form_questions
                    SET title = 'Global_CheckYourAnswers'
                    WHERE name = '_Qualifications04';

                    UPDATE form_sections
                    SET configuration = '{{""AddAnotherAnswerLabel"": ""Qualifications_Configuration_AddAnotherAnswerLabel"", ""SingularSummaryHeading"": ""Qualifications_Configuration_SingularSummaryHeading"", ""RemoveConfirmationCaption"": ""Qualifications_SectionTitle"", ""RemoveConfirmationHeading"": ""Qualifications_Configuration_RemoveConfirmationHeading"", ""PluralSummaryHeadingFormat"": ""Qualifications_Configuration_PluralSummaryHeadingFormat"", ""FurtherQuestionsExemptedHint"": ""Qualifications_Configuration_FurtherQuestionsExemptedHint"", ""FurtherQuestionsExemptedHeading"": ""Qualifications_Configuration_FurtherQuestionsExemptedHeading""}}'
                    WHERE title = 'Qualifications_SectionTitle';
                END $$;
             ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
