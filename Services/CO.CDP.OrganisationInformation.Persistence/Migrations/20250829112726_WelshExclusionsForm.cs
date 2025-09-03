using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class WelshExclusionsForm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                DO $$
                DECLARE
                    form_id int;
                    sectionId INT;
                    previousQuestionId INT;
                BEGIN
                    SELECT id INTO form_id FROM forms WHERE name = 'Standard Questions';

	                INSERT INTO form_sections (guid, title, form_id, allows_multiple_answer_sets, check_further_questions_exempted, type,display_order, configuration)
                    VALUES ('{Guid.NewGuid()}', 'WelshExclusions_SectionTitle', form_id, FALSE, FALSE, 3,1, '{{""AddAnotherAnswerLabel"": ""WelshExclusions_Configuration_AddAnotherAnswerLabel"", ""SingularSummaryHeading"": ""WelshExclusions_Configuration_SingularSummaryHeading"", ""SummaryRenderFormatter"": {{""KeyParams"": [""_WelshExclusions02"", ""_WelshExclusions04""], ""ValueParams"": [""_WelshExclusions02"", ""_WelshExclusions04""], ""KeyExpression"": ""{{0}}"", ""ValueExpression"": ""{{1}}"", ""KeyExpressionOperation"": ""StringFormat"", ""ValueExpressionOperation"": ""StringFormat""}}, ""RemoveConfirmationCaption"": ""WelshExclusions_SectionTitle"", ""RemoveConfirmationHeading"": ""WelshExclusions_Configuration_RemoveConfirmationHeading"", ""PluralSummaryHeadingFormat"": ""WelshExclusions_Configuration_PluralSummaryHeadingFormat"", ""SingularSummaryHeadingHint"": ""WelshExclusions_Configuration_SingularSummaryHeadingHint"", ""FurtherQuestionsExemptedHint"": ""WelshExclusions_Configuration_FurtherQuestionsExemptedHint"", ""PluralSummaryHeadingHintFormat"": ""WelshExclusions_Configuration_PluralSummaryHeadingHintFormat"", ""FurtherQuestionsExemptedHeading"": ""WelshExclusions_Configuration_FurtherQuestionsExemptedHeading""}}');

                    SELECT id INTO sectionId FROM form_sections where title = 'WelshExclusions_SectionTitle';

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, sectionId, 6, true, 'Global_CheckYourAnswers', '', '{{}}', NULL, NULL, '_WelshExclusions06', 6)
                    RETURNING id INTO previousQuestionId;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', previousQuestionId, NULL, sectionId, 10, false, 'WelshExclusions_05_Title', NULL, '{{}}', NULL, 'WelshExclusions_05_SummaryTitle', '_WelshExclusions05', 5)
                    RETURNING id INTO previousQuestionId;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', previousQuestionId, NULL, sectionId, 3, false, 'WelshExclusions_04_Title', NULL, '{{}}', NULL, 'WelshExclusions_04_SummaryTitle', '_WelshExclusions04', 4)
                    RETURNING id INTO previousQuestionId;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', previousQuestionId, NULL, sectionId, 7, false, 'WelshExclusions_03_Title', NULL, '{{}}', NULL, 'WelshExclusions_03_SummaryTitle', '_WelshExclusions03', 3)
                    RETURNING id INTO previousQuestionId;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', previousQuestionId, NULL, sectionId, 3, true, 'WelshExclusions_02_Title', 'WelshExclusions_02_Description', '{{}}', NULL, 'WelshExclusions_02_SummaryTitle', '_WelshExclusions02', 2)
                    RETURNING id INTO previousQuestionId;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', previousQuestionId, NULL, sectionId, 0, true, 'WelshExclusions_01_Title', 'WelshExclusions_01_Description', '{{}}', NULL, '', '_WelshExclusions01', 1)
                    RETURNING id INTO previousQuestionId;

                END $$;
            ");
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                DO $$
                DECLARE
                    sectionId INT;
                BEGIN
                    SELECT id INTO sectionId FROM form_sections WHERE title = 'WelshExclusions_SectionTitle';
                    DELETE FROM form_questions WHERE section_id = sectionId;
                    DELETE FROM form_sections WHERE id = sectionId;
                END $$;
            ");
        }
    }
}
