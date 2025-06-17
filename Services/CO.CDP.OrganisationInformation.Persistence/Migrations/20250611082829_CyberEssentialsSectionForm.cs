using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CyberEssentialsSectionForm : Migration
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
                    VALUES ('{Guid.NewGuid()}', 'CyberEssentials_SectionTitle', form_id, FALSE, FALSE, 3,1, '{{""AddAnotherAnswerLabel"": ""CyberEssentials_Configuration_AddAnotherAnswerLabel"", ""SingularSummaryHeading"": ""CyberEssentials_Configuration_SingularSummaryHeading"", ""SummaryRenderFormatter"": {{""KeyParams"": [""_CyberEssentialsZeroQuestion01"", ""_CyberEssentials02""], ""ValueParams"": [""_CyberEssentials01"", ""_CyberEssentials02""], ""KeyExpression"": ""{{0}}"", ""ValueExpression"": ""{{1}}"", ""KeyExpressionOperation"": ""StringFormat"", ""ValueExpressionOperation"": ""StringFormat""}}, ""RemoveConfirmationCaption"": ""CyberEssentials_SectionTitle"", ""RemoveConfirmationHeading"": ""CyberEssentials_Configuration_RemoveConfirmationHeading"", ""PluralSummaryHeadingFormat"": ""CyberEssentials_Configuration_PluralSummaryHeadingFormat"", ""SingularSummaryHeadingHint"": ""CyberEssentials_Configuration_SingularSummaryHeadingHint"", ""FurtherQuestionsExemptedHint"": ""CyberEssentials_Configuration_FurtherQuestionsExemptedHint"", ""PluralSummaryHeadingHintFormat"": ""CyberEssentials_Configuration_PluralSummaryHeadingHintFormat"", ""FurtherQuestionsExemptedHeading"": ""CyberEssentials_Configuration_FurtherQuestionsExemptedHeading""}}');

                    SELECT id INTO sectionId FROM form_sections where title = 'CyberEssentials_SectionTitle';

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, sectionId, 6, true, 'Global_CheckYourAnswers', '', '{{}}', NULL, NULL, '_CyberEssentials10', 10)
                    RETURNING id INTO previousQuestionId;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', previousQuestionId, NULL, sectionId, 7, true, 'CyberEssentials_09_Title', NULL, '{{}}', NULL, 'CyberEssentials_09_SummaryTitle', '_CyberEssentials09', 9)
                    RETURNING id INTO previousQuestionId;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', previousQuestionId, NULL, sectionId, 2, true, 'CyberEssentials_08_Title', NULL, '{{}}', NULL, 'CyberEssentials_08_SummaryTitle', '_CyberEssentials08', 8)
                    RETURNING id INTO previousQuestionId;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', previousQuestionId, NULL, sectionId, 3, true, 'CyberEssentials_07_Title', '', '{{}}', NULL, 'CyberEssentials_07_SummaryTitle', '_CyberEssentials07', 7)
                    RETURNING id INTO previousQuestionId;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', previousQuestionId, NULL, sectionId, 3, true, 'CyberEssentials_06_Title', '', '{{}}', NULL, 'CyberEssentials_06_SummaryTitle', '_CyberEssentials06', 6)
                    RETURNING id INTO previousQuestionId;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', previousQuestionId, NULL, sectionId, 7, true, 'CyberEssentials_05_Title', NULL, '{{}}', NULL, 'CyberEssentials_05_SummaryTitle', '_CyberEssentials05', 5)
                    RETURNING id INTO previousQuestionId;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', previousQuestionId, NULL, sectionId, 2, true, 'CyberEssentials_04_Title', NULL, '{{}}', NULL, 'CyberEssentials_04_SummaryTitle', '_CyberEssentials04', 4)
                    RETURNING id INTO previousQuestionId;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', previousQuestionId, NULL, sectionId, 3, true, 'CyberEssentials_03_Title', NULL, '{{}}', NULL, 'CyberEssentials_03_SummaryTitle', '_CyberEssentials03', 3)
                    RETURNING id INTO previousQuestionId;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', previousQuestionId, NULL, sectionId, 3, true, 'CyberEssentials_02_Title', NULL, '{{}}', NULL, 'CyberEssentials_02_SummaryTitle', '_CyberEssentials02', 2)
                    RETURNING id INTO previousQuestionId;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', previousQuestionId, NULL, sectionId, 0, true, 'CyberEssentials_01_Title', 'CyberEssentials_01_Description', '{{}}', NULL, NULL, '_CyberEssentials01', 1)
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
                    SELECT id INTO sectionId FROM form_sections WHERE title = 'CyberEssentials_SectionTitle';
                    DELETE FROM form_questions WHERE section_id = sectionId;
                    DELETE FROM form_sections WHERE id = sectionId;
                END $$;
            ");
        }
    }
}