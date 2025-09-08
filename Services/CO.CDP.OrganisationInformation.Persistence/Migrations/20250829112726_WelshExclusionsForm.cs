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
            v_form_id INTEGER;
            v_section_id INTEGER;
            v_question1_id INTEGER;
            v_question2_id INTEGER;
            v_question3_id INTEGER;
            v_question4_id INTEGER;
            v_question5_id INTEGER;
            v_question6_id INTEGER;
            debug_next_id INTEGER;
            debug_alt_id INTEGER;
        BEGIN
            SELECT id INTO v_form_id FROM forms WHERE name = 'Standard Questions';

            IF v_form_id IS NULL THEN
                RAISE EXCEPTION 'Form ""Standard Questions"" not found. Migration cannot proceed.';
            END IF;

	        INSERT INTO form_sections (guid, title, form_id, allows_multiple_answer_sets, check_further_questions_exempted, ""type"",display_order, configuration)
            VALUES ('{Guid.NewGuid()}', 'WelshExclusions_SectionTitle', v_form_id, FALSE, FALSE, 4,1, '{{""AddAnotherAnswerLabel"": ""WelshExclusions_Configuration_AddAnotherAnswerLabel"", ""SingularSummaryHeading"": ""WelshExclusions_Configuration_SingularSummaryHeading"", ""SummaryRenderFormatter"": {{""KeyParams"": [""_WelshExclusions02"", ""_WelshExclusions04""], ""ValueParams"": [""_WelshExclusions02"", ""_WelshExclusions04""], ""KeyExpression"": ""{{0}}"", ""ValueExpression"": ""{{1}}"", ""KeyExpressionOperation"": ""StringFormat"", ""ValueExpressionOperation"": ""StringFormat""}}, ""RemoveConfirmationCaption"": ""WelshExclusions_SectionTitle"", ""RemoveConfirmationHeading"": ""WelshExclusions_Configuration_RemoveConfirmationHeading"", ""PluralSummaryHeadingFormat"": ""WelshExclusions_Configuration_PluralSummaryHeadingFormat"", ""SingularSummaryHeadingHint"": ""WelshExclusions_Configuration_SingularSummaryHeadingHint"", ""FurtherQuestionsExemptedHint"": ""WelshExclusions_Configuration_FurtherQuestionsExemptedHint"", ""PluralSummaryHeadingHintFormat"": ""WelshExclusions_Configuration_PluralSummaryHeadingHintFormat"", ""FurtherQuestionsExemptedHeading"": ""WelshExclusions_Configuration_FurtherQuestionsExemptedHeading""}}')
            RETURNING id INTO v_section_id;

            INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
            VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 0, true, 'WelshExclusions_01_Title', 'WelshExclusions_01_Description', '{{}}', NULL, '', '_WelshExclusions01', 1)
            RETURNING id INTO v_question1_id;

            INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
            VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 3, true, 'WelshExclusions_02_Title', 'WelshExclusions_02_Description', '{{}}', NULL, 'WelshExclusions_02_SummaryTitle', '_WelshExclusions02', 2)
            RETURNING id INTO v_question2_id;

            INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
            VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 7, true, 'WelshExclusions_03_Title', NULL, '{{}}', NULL, 'WelshExclusions_03_SummaryTitle', '_WelshExclusions03', 3)
            RETURNING id INTO v_question3_id;

            INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
            VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 3, false, 'WelshExclusions_04_Title', NULL, '{{}}', NULL, 'WelshExclusions_04_SummaryTitle', '_WelshExclusions04', 4)
            RETURNING id INTO v_question4_id;

            INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
            VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 10, false, 'WelshExclusions_05_Title', NULL, '{{}}', NULL, 'WelshExclusions_05_SummaryTitle', '_WelshExclusions05', 5)
            RETURNING id INTO v_question5_id;

            INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
            VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 6, true, 'Global_CheckYourAnswers', '', '{{}}', NULL, NULL, '_WelshExclusions06', 6)
            RETURNING id INTO v_question6_id;

            -- Update the next question links
            UPDATE form_questions SET next_question_id = v_question2_id WHERE title = 'WelshExclusions_01_Title' AND section_id = v_section_id;
            UPDATE form_questions SET next_question_id = v_question3_id, next_question_alternative_id = v_question6_id WHERE title = 'WelshExclusions_02_Title' AND section_id = v_section_id;
            UPDATE form_questions SET next_question_id = v_question4_id WHERE title = 'WelshExclusions_03_Title' AND section_id = v_section_id;
            UPDATE form_questions SET next_question_id = v_question5_id WHERE title = 'WelshExclusions_04_Title' AND section_id = v_section_id;
            UPDATE form_questions SET next_question_id = v_question6_id WHERE title = 'WelshExclusions_05_Title' AND section_id = v_section_id;

        END $$;
    ");
}



        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                DO $$
                DECLARE
                    v_section_id INT;
                BEGIN
                    SELECT id INTO v_section_id FROM form_sections WHERE title = 'WelshExclusions_SectionTitle';

                    IF v_section_id IS NOT NULL THEN
                        DELETE FROM form_questions WHERE section_id = v_section_id;
                        DELETE FROM form_sections WHERE id = v_section_id;
                    END IF;
                END $$;
            ");
        }
    }
}
