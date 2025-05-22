using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class HealthAndSafetySectionForm : Migration
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
                    VALUES ('{Guid.NewGuid()}', 'HealthAndSafety_SectionTitle', form_id, FALSE, FALSE, 3,1, '{{""AddAnotherAnswerLabel"": ""HealthAndSafety_Configuration_AddAnotherAnswerLabel"", ""SingularSummaryHeading"": ""HealthAndSafety_Configuration_SingularSummaryHeading"", ""SummaryRenderFormatter"": {{""KeyParams"": [""_HealthAndSafetyQuestion02"", ""_HealthAndSafetyQuestion04""], ""ValueParams"": [""_HealthAndSafetyQuestion02"", ""_HealthAndSafetyQuestion04""], ""KeyExpression"": ""{{0}}"", ""ValueExpression"": ""{{1}}"", ""KeyExpressionOperation"": ""StringFormat"", ""ValueExpressionOperation"": ""StringFormat""}}, ""RemoveConfirmationCaption"": ""HealthAndSafety_SectionTitle"", ""RemoveConfirmationHeading"": ""HealthAndSafety_Configuration_RemoveConfirmationHeading"", ""PluralSummaryHeadingFormat"": ""HealthAndSafety_Configuration_PluralSummaryHeadingFormat"", ""SingularSummaryHeadingHint"": ""HealthAndSafety_Configuration_SingularSummaryHeadingHint"", ""FurtherQuestionsExemptedHint"": ""HealthAndSafety_Configuration_FurtherQuestionsExemptedHint"", ""PluralSummaryHeadingHintFormat"": ""HealthAndSafety_Configuration_PluralSummaryHeadingHintFormat"", ""FurtherQuestionsExemptedHeading"": ""HealthAndSafety_Configuration_FurtherQuestionsExemptedHeading""}}');

                    SELECT id INTO sectionId FROM form_sections where title = 'HealthAndSafety_SectionTitle';

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', previousQuestionId, NULL, sectionId, 10, true, 'HealthAndSafetyQuestion_04_Title', 'HealthAndSafetyQuestion_04_Description', '{{}}', NULL, '', '_HealthAndSafetyQuestion04', 4)
                    RETURNING id INTO previousQuestionId;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', previousQuestionId, NULL, sectionId, 2, false, 'HealthAndSafetyQuestion_03_Title', NULL, '{{}}', NULL, '', '_HealthAndSafetyQuestion03', 3)
                    RETURNING id INTO previousQuestionId;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', previousQuestionId, NULL, sectionId, 10, true, 'HealthAndSafetyQuestion_02_Title', 'HealthAndSafetyQuestion_02_Description', '{{}}', NULL, '', '_HealthAndSafetyQuestion02', 2)
                    RETURNING id INTO previousQuestionId;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', previousQuestionId, NULL, sectionId, 0, true, 'HealthAndSafetyQuestion_01_Title', 'HealthAndSafetyQuestion_01_Description', '{{}}', NULL, '', '_HealthAndSafetyQuestion01', 1)
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
                    SELECT id INTO sectionId FROM form_sections WHERE title = 'HealthAndSafety_SectionTitle';
                    DELETE FROM form_questions WHERE section_id = sectionId;
                    DELETE FROM form_sections WHERE id = sectionId;
                END $$;
            ");
        }
    }
}
