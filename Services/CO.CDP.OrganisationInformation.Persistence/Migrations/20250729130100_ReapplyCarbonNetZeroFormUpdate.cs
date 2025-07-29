using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReapplyCarbonNetZeroFormUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                DO $$
                DECLARE
                    form_id int;
                    sectionId INT;
                    question1_id INT;
                    question2_id INT;
                    question3_id INT;
                    question4_id INT;
                    question5_id INT;
                    question6_id INT;
                    question7_id INT;
                    question8_id INT;
                    question9_id INT;
                    question10_id INT;
                    question11_id INT;
                    question12_id INT;
                    question13_id INT;
                    question14_id INT;


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
                    VALUES('{Guid.NewGuid()}', NULL, NULL, sectionId, 12, false, 'CarbonNetZero_03_Title', NULL, '{{}}', NULL, 'CarbonNetZero_03_SummaryTitle', '_CarbonNetZeroQuestion03', 3)
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
                    VALUES('{Guid.NewGuid()}', NULL, NULL, sectionId, 1, true, 'CarbonNetZero_06_Title', '', '{{}}', NULL, 'CarbonNetZero_06_SummaryTitle', '_CarbonNetZeroQuestion06', 6)
                    RETURNING id INTO question6_id;

                    -- Insert the seventh question (_CarbonNetZeroQuestion07)
                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, sectionId, 1, true, 'CarbonNetZero_07_Title', '', '{{}}', NULL, 'CarbonNetZero_07_SummaryTitle', '_CarbonNetZeroQuestion07', 7)
                    RETURNING id INTO question7_id;

                    -- Insert the eighth question (_CarbonNetZeroQuestion08)
                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, sectionId, 1, true, 'CarbonNetZero_08_Title', '', '{{}}', NULL, 'CarbonNetZero_08_SummaryTitle', '_CarbonNetZeroQuestion08', 8)
                    RETURNING id INTO question8_id;

                    -- Insert the ninth question (_CarbonNetZeroQuestion09)
                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, sectionId, 1, true, 'CarbonNetZero_09_Title', '', '{{}}', NULL, 'CarbonNetZero_09_SummaryTitle', '_CarbonNetZeroQuestion09', 9)
                    RETURNING id INTO question9_id;

                    -- Insert the tenth question (_CarbonNetZeroQuestion10)
                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, sectionId, 1, true, 'CarbonNetZero_10_Title', '', '{{}}', NULL, 'CarbonNetZero_10_SummaryTitle', '_CarbonNetZeroQuestion10', 10)
                    RETURNING id INTO question10_id;

                    -- Insert the eleventh question (_CarbonNetZeroQuestion11)
                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, sectionId, 1, true, 'CarbonNetZero_11_Title', '', '{{}}', NULL, 'CarbonNetZero_11_SummaryTitle', '_CarbonNetZeroQuestion11', 11)
                    RETURNING id INTO question11_id;

                    -- Insert the twelfth question (_CarbonNetZeroQuestion12)
                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, sectionId, 1, true, 'CarbonNetZero_12_Title', '', '{{}}', NULL, 'CarbonNetZero_12_SummaryTitle', '_CarbonNetZeroQuestion12', 12)
                    RETURNING id INTO question12_id;

                    -- Insert the thirteenth question (_CarbonNetZeroQuestion13)
                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, sectionId, 1, true, 'CarbonNetZero_13_Title', '', '{{}}', NULL, 'CarbonNetZero_13_SummaryTitle', '_CarbonNetZeroQuestion13', 13)
                    RETURNING id INTO question13_id;

                    -- Insert the fourteenth question (_CarbonNetZeroQuestion14)
                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, sectionId, 6, true, 'Global_CheckYourAnswers', NULL, '{{}}', NULL, NULL, '_CarbonNetZeroQuestion14', 14)
                    RETURNING id INTO question14_id;

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

                    -- Link Q6 to Q7
                    UPDATE form_questions
                    SET next_question_id = question7_id
                    WHERE id = question6_id;

                    -- Link Q7 to Q8
                    UPDATE form_questions
                    SET next_question_id = question8_id
                    WHERE id = question7_id;

                    -- Link Q8 to Q9
                    UPDATE form_questions
                    SET next_question_id = question9_id
                    WHERE id = question8_id;

                    -- Link Q9 to Q10
                    UPDATE form_questions
                    SET next_question_id = question10_id
                    WHERE id = question9_id;

                    -- Link Q10 to Q11
                    UPDATE form_questions
                    SET next_question_id = question11_id
                    WHERE id = question10_id;

                    -- Link Q11 to Q12
                    UPDATE form_questions
                    SET next_question_id = question12_id
                    WHERE id = question11_id;

                    -- Link Q12 to Q13
                    UPDATE form_questions
                    SET next_question_id = question13_id
                    WHERE id = question12_id;

                    -- Link Q13 to Q14
                    UPDATE form_questions
                    SET next_question_id = question14_id
                    WHERE id = question13_id;

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
                    DELETE FROM form_questions WHERE section_id = sectionId;
                    DELETE FROM form_sections WHERE id = sectionId;
                END $$;
            ");
        }
    }
}
