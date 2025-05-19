using Microsoft.EntityFrameworkCore.Migrations;
using System; // Required for Guid

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CarbonNetZeroSectionForm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                DO $$
                DECLARE
                    form_id int;
                    sectionId INT;
                    question1_id INT; -- To store the ID of the first question
                    question2_id INT; -- To store the ID of the second question
                    question3_id INT; -- To store the ID of the third question
                    question4_id INT; -- To store the ID of the fourth question
                    question5_id INT; -- To store the ID of the fifth question
                    question6_id INT; -- To store the ID of the sixth question


                BEGIN
                    SELECT id INTO form_id FROM forms WHERE name = 'Standard Questions';

                    -- Insert the form section
                    INSERT INTO form_sections (guid, title, form_id, allows_multiple_answer_sets, check_further_questions_exempted, type,display_order, configuration)
                    VALUES ('{Guid.NewGuid()}', 'CarbonNetZero_SectionTitle', form_id, FALSE, FALSE, 3,1, '{{""AddAnotherAnswerLabel"": ""CarbonNetZero_Configuration_AddAnotherAnswerLabel"", ""SingularSummaryHeading"": ""CarbonNetZero_Configuration_SingularSummaryHeading"", ""SummaryRenderFormatter"": {{""KeyParams"": [""_CarbonNetZeroQuestion01"", ""_CarbonNetZeroQuestion02""], ""ValueParams"": [""_CarbonNetZeroQuestion01"", ""_CarbonNetZeroQuestion02""], ""KeyExpression"": ""{{0}}"", ""ValueExpression"": ""{{1}}"", ""KeyExpressionOperation"": ""StringFormat"", ""ValueExpressionOperation"": ""StringFormat""}}, ""RemoveConfirmationCaption"": ""CarbonNetZero_SectionTitle"", ""RemoveConfirmationHeading"": ""CarbonNetZero_Configuration_RemoveConfirmationHeading"", ""PluralSummaryHeadingFormat"": ""CarbonNetZero_Configuration_PluralSummaryHeadingFormat"", ""SingularSummaryHeadingHint"": ""CarbonNetZero_Configuration_SingularSummaryHeadingHint"", ""FurtherQuestionsExemptedHint"": ""CarbonNetZero_Configuration_FurtherQuestionsExemptedHint"", ""PluralSummaryHeadingHintFormat"": ""CarbonNetZero_Configuration_PluralSummaryHeadingHintFormat"", ""FurtherQuestionsExemptedHeading"": ""CarbonNetZero_Configuration_FurtherQuestionsExemptedHeading""}}');

                    SELECT id INTO sectionId FROM form_sections where title = 'CarbonNetZero_SectionTitle';

                    -- Insert the first question (_CarbonNetZeroQuestion01)
                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, sectionId, 0, true, 'CarbonNetZero_01_Title', 'CarbonNetZero_01_Description', '{{}}', NULL, 'CarbonNetZero_01_SummaryTitle', '_CarbonNetZeroQuestion01', 1)
                    RETURNING id INTO question1_id;

                    -- Insert the second question (_CarbonNetZeroQuestion02)
                    -- next_question_alternative_id will be set in a subsequent UPDATE
                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, sectionId, 3, true, 'CarbonNetZero_02_Title', 'CarbonNetZero_02_Description', '{{}}', NULL, 'CarbonNetZero_02_SummaryTitle', '_CarbonNetZeroQuestion02', 2)
                    RETURNING id INTO question2_id;

                    -- Insert the third question (_CarbonNetZeroQuestion03) - This is the 'happy path' from Q2
                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, sectionId, 12, true, 'CarbonNetZero_03_Title', NULL, '{{}}', NULL, 'CarbonNetZero_03_SummaryTitle', '_CarbonNetZeroQuestion03', 3)
                    RETURNING id INTO question3_id;

                    -- Insert the fourth question (_CarbonNetZeroQuestion04)
                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, sectionId, 7, true, 'CarbonNetZero_04_Title', NULL, '{{}}', NULL, 'CarbonNetZero_04_SummaryTitle', '_CarbonNetZeroQuestion04', 4)
                    RETURNING id INTO question4_id;

                    -- Insert the fifth question (_CarbonNetZeroQuestion05)
                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, sectionId, 1, true, 'CarbonNetZero_05_Title', 'CarbonNetZero_05_Description', '{{}}', NULL, 'CarbonNetZero_05_SummaryTitle', '_CarbonNetZeroQuestion05', 5)
                    RETURNING id INTO question5_id;

                    -- Insert the sixth question (_CarbonNetZeroQuestion06)
                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, sectionId, 6, true, 'Global_CheckYourAnswers', NULL, '{{}}', NULL, NULL, '_CarbonNetZeroQuestion06', 6)
                    RETURNING id INTO question6_id;

                    -- Link Q1 to Q2
                    UPDATE form_questions
                    SET next_question_id = question2_id
                    WHERE id = question1_id;

                    -- Link Q2 to Q3
                    UPDATE form_questions
                    SET next_question_id = question3_id
                    WHERE id = question2_id;

                    -- Link Q3 to Q4
                    UPDATE form_questions
                    SET next_question_id = question4_id
                    WHERE id = question3_id;

                    -- Link Q4 to Q5
                    UPDATE form_questions
                    SET next_question_id = question5_id
                    WHERE id = question4_id;

                    -- Link Q5 to Q6
                    UPDATE form_questions
                    SET next_question_id = question6_id
                    WHERE id = question5_id;

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
                    SELECT id INTO sectionId FROM form_sections WHERE title = 'CarbonNetZero_SectionTitle';
                    DELETE FROM form_questions WHERE section_id = sectionId; -- This will delete all questions in the section
                    DELETE FROM form_sections WHERE id = sectionId;
                END $$;
            ");
        }
    }
}
