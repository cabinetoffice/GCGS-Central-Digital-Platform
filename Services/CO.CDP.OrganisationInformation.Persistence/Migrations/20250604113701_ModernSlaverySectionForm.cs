using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ModernSlaverySectionForm : Migration
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
                        question4Id INT;
                    BEGIN
                        SELECT id INTO form_id FROM forms WHERE name = 'Standard Questions';

                        INSERT INTO form_sections (guid, title, form_id, allows_multiple_answer_sets, check_further_questions_exempted, type,display_order, configuration)
                        VALUES ('{Guid.NewGuid()}', 'ModernSlavery_SectionTitle', form_id, FALSE, FALSE, 3,1, '{{""AddAnotherAnswerLabel"": ""ModernSlavery_Configuration_AddAnotherAnswerLabel"", ""SingularSummaryHeading"": ""ModernSlavery_Configuration_SingularSummaryHeading"", ""SummaryRenderFormatter"": {{""KeyParams"": [""_ModernSlaveryQuestion02"", ""_ModernSlavery04""], ""ValueParams"": [""_ModernSlavery02"", ""_ModernSlaveryQuestion04""], ""KeyExpression"": ""{{0}}"", ""ValueExpression"": ""{{1}}"", ""KeyExpressionOperation"": ""StringFormat"", ""ValueExpressionOperation"": ""StringFormat""}}, ""RemoveConfirmationCaption"": ""ModernSlavery_SectionTitle"", ""RemoveConfirmationHeading"": ""ModernSlavery_Configuration_RemoveConfirmationHeading"", ""PluralSummaryHeadingFormat"": ""ModernSlavery_Configuration_PluralSummaryHeadingFormat"", ""SingularSummaryHeadingHint"": ""ModernSlavery_Configuration_SingularSummaryHeadingHint"", ""FurtherQuestionsExemptedHint"": ""ModernSlavery_Configuration_FurtherQuestionsExemptedHint"", ""PluralSummaryHeadingHintFormat"": ""ModernSlavery_Configuration_PluralSummaryHeadingHintFormat"", ""FurtherQuestionsExemptedHeading"": ""ModernSlavery_Configuration_FurtherQuestionsExemptedHeading""}}');

                        SELECT id INTO sectionId FROM form_sections where title = 'ModernSlavery_SectionTitle';

                        INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                        VALUES('{Guid.NewGuid()}', previousQuestionId, NULL, sectionId, 6, true, 'Global_CheckYourAnswers', NULL, '{{}}', NULL, '', '_ModernSlaveryQuestion12', 12)
                        RETURNING id INTO previousQuestionId;

                        INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                        VALUES('{Guid.NewGuid()}', previousQuestionId, NULL, sectionId, 10, true, 'ModernSlavery_11_Title', 'ModernSlavery_11_Description', '{{}}', NULL, 'ModernSlavery_11_SummaryTitle', '_ModernSlaveryQuestion11', 11)
                        RETURNING id INTO previousQuestionId;

                        INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                        VALUES('{Guid.NewGuid()}', previousQuestionId, NULL, sectionId, 10, true, 'ModernSlavery_10_Title', 'ModernSlavery_10_Description', '{{}}', NULL, '', '_ModernSlaveryQuestion10', 10)
                        RETURNING id INTO previousQuestionId;

                        INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                        VALUES('{Guid.NewGuid()}', previousQuestionId, NULL, sectionId, 10, true, 'ModernSlavery_09_Title', 'ModernSlavery_09_Description', '{{}}', NULL, '', '_ModernSlaveryQuestion09', 9)
                        RETURNING id INTO previousQuestionId;

                        INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                        VALUES('{Guid.NewGuid()}', previousQuestionId, NULL, sectionId, 2, true, 'ModernSlavery_08_Title', NULL, '{{}}', NULL, '', '_ModernSlaveryQuestion08', 8)
                        RETURNING id INTO previousQuestionId;

                        INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                        VALUES('{Guid.NewGuid()}', previousQuestionId, NULL, sectionId, 12, true, 'ModernSlavery_07_Title', NULL, '{{}}', NULL, '', '_ModernSlaveryQuestion07', 7)
                        RETURNING id INTO previousQuestionId;

                        INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                        VALUES('{Guid.NewGuid()}', previousQuestionId, NULL, sectionId, 2, true, 'ModernSlavery_06_Title', '', '{{}}', NULL, '', '_ModernSlaveryQuestion06', 6)
                        RETURNING id INTO previousQuestionId;

                        INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                        VALUES('{Guid.NewGuid()}', previousQuestionId, NULL, sectionId, 12, true, 'ModernSlavery_05_Title', 'ModernSlavery_05_Description', '{{}}', NULL, '', '_ModernSlaveryQuestion05', 5)
                        RETURNING id INTO previousQuestionId;

                        INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                        VALUES('{Guid.NewGuid()}', previousQuestionId, NULL, sectionId, 3, true, 'ModernSlavery_04_Title', 'ModernSlavery_04_Description', '{{}}', NULL, '', '_ModernSlaveryQuestion04', 4)
                        RETURNING id INTO previousQuestionId;
                        question4Id := previousQuestionId;

                        INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                        VALUES('{Guid.NewGuid()}', previousQuestionId, NULL, sectionId, 3, true, 'ModernSlavery_03_Title', '', '{{}}', NULL, '', '_ModernSlaveryQuestion03', 3)
                        RETURNING id INTO previousQuestionId;

                        INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                        VALUES('{Guid.NewGuid()}', previousQuestionId, question4Id, sectionId, 3, true, 'ModernSlavery_02_Title', 'ModernSlavery_02_Description', '{{}}', NULL, '', '_ModernSlaveryQuestion02', 2)
                        RETURNING id INTO previousQuestionId;

                        INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                        VALUES('{Guid.NewGuid()}', previousQuestionId, NULL, sectionId, 0, true, 'ModernSlavery_01_Title', 'ModernSlavery_01_Description', '{{}}', NULL, '', '_ModernSlaveryQuestion01', 1)
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
                    -- Get the ID of the section for Modern Slavery
                    SELECT id INTO sectionId FROM form_sections WHERE title = 'ModernSlavery_SectionTitle';

                    -- Delete the Modern Slavery section
                    DELETE FROM form_sections WHERE id = sectionId;
                END $$;
            ");
        }
    }
}
