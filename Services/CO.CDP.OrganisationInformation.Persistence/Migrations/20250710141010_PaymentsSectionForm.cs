using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PaymentsSectionForm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                DO $$
                DECLARE
                    v_form_id int;
                    v_section_id INT;
                    v_question1_id INT;
                    v_question2_id INT;
                    v_question3_id INT;
                    v_question4_id INT;
                    v_question5_id INT;
                    v_question6_id INT;
                    v_question7_id INT;
                    v_question8_id INT;
                    v_question9_id INT;
                    v_question10_id INT;
                    v_question11_id INT;
                    v_question12_id INT;
                BEGIN
                    SELECT id INTO v_form_id FROM forms WHERE name = 'Standard Questions';

                    IF v_form_id IS NULL THEN
                        RAISE EXCEPTION 'Form ""Standard Questions"" not found. Migration cannot proceed.';
                    END IF;

	                INSERT INTO form_sections (guid, title, form_id, allows_multiple_answer_sets, check_further_questions_exempted, type,display_order, configuration)
                    VALUES ('{Guid.NewGuid()}', 'Payments_SectionTitle', v_form_id, FALSE, FALSE, 3,1, '{{}}')
                    RETURNING id INTO v_section_id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"", caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 0, true, 'Payments_01_Title', 'Payments_01_Description', '{{}}', NULL, '', '_Payments01', 1)
                    RETURNING id INTO v_question1_id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"", caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 3, true, 'Payments_02_Title', 'Payments_02_Description', '{{}}', NULL, 'Payments_02_SummaryTitle', '_Payments02', 2)
                    RETURNING id INTO v_question2_id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"", caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 3, true, 'Payments_03_Title', 'Payments_03_Description', '{{}}', NULL, 'Payments_03_SummaryTitle', '_Payments03', 3)
                    RETURNING id INTO v_question3_id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"", caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 3, true, 'Payments_04_Title', 'Payments_04_Description', '{{}}', NULL, 'Payments_04_SummaryTitle', '_Payments04', 4)
                    RETURNING id INTO v_question4_id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"", caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 12, false, 'Payments_05_Title', NULL, '{{}}', NULL, 'Payments_05_SummaryTitle', '_Payments05', 5)
                    RETURNING id INTO v_question5_id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 3, true, 'Payments_06_Title', 'Payments_06_Description', '{{}}', NULL, '', '_Payments06', 6)
                    RETURNING id INTO v_question6_id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 3, true, 'Payments_07_Title', 'Payments_07_Description', '{{}}', NULL, '', '_Payments07', 7)
                    RETURNING id INTO v_question7_id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 10, true, 'Payments_08_Title', 'Payments_08_Description', '{{}}', NULL, '', '_Payments08', 8)
                    RETURNING id INTO v_question8_id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 3, true, 'Payments_09_Title', 'Payments_09_Description', '{{}}', NULL, 'Payments_09_SummaryTitle', '_Payments09', 9)
                    RETURNING id INTO v_question9_id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 3, true, 'Payments_10_Title', 'Payments_10_Description', '{{}}', NULL, 'Payments_10_SummaryTitle', '_Payments10', 10)
                    RETURNING id INTO v_question10_id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 10, true, 'Payments_11_Title', 'Payments_11_Description', '{{}}', NULL, 'Payments_11_SummaryTitle', '_Payments11', 11)
                    RETURNING id INTO v_question11_id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"", caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 6, true, 'Global_CheckYourAnswers', NULL, '{{}}', NULL, '', '_Payments12', 12)
                    RETURNING id INTO v_question12_id;

                    -- Update the next question links
                    UPDATE form_questions SET next_question_id = v_question2_id WHERE title = 'Payments_01_Title' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question3_id WHERE title = 'Payments_02_Title' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question4_id WHERE title = 'Payments_03_Title' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question5_id WHERE title = 'Payments_04_Title' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question6_id WHERE title = 'Payments_05_Title' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question7_id WHERE title = 'Payments_06_Title' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question8_id WHERE title = 'Payments_07_Title' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question9_id WHERE title = 'Payments_08_Title' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question10_id WHERE title = 'Payments_09_Title' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question11_id WHERE title = 'Payments_10_Title' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question12_id WHERE title = 'Payments_11_Title' AND section_id = v_section_id;

                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                DO $$
                DECLARE
                    v_section_id INTEGER;
                BEGIN
                    -- Get the ID of the section for Payments
                    SELECT id INTO v_section_id FROM form_sections WHERE title = 'Payments_SectionTitle';

                    IF v_section_id IS NOT NULL THEN
                        -- Delete the questions associated with the section first
                        DELETE FROM form_questions WHERE section_id = v_section_id;
                        -- Delete the Payments section
                        DELETE FROM form_sections WHERE id = v_section_id;
                    END IF;
                END $$;
            ");
        }
    }
}
