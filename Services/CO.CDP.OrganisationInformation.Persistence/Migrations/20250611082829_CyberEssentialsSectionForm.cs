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
                    v_form_id INTEGER;
                    v_section_id INTEGER;
                    v_question1_id INTEGER;
                    v_question2_id INTEGER;
                    v_question3_id INTEGER;
                    v_question4_id INTEGER;
                    v_question5_id INTEGER;
                    v_question6_id INTEGER;
                    v_question7_id INTEGER;
                    v_question8_id INTEGER;
                    v_question9_id INTEGER;
                    v_question10_id INTEGER;
                BEGIN
                    SELECT id INTO v_form_id FROM forms WHERE name = 'Standard Questions';

                    IF v_form_id IS NULL THEN
                        RAISE EXCEPTION 'Form ""Standard Questions"" not found. Migration cannot proceed.';
                    END IF;

                    INSERT INTO form_sections (guid, title, form_id, allows_multiple_answer_sets, check_further_questions_exempted, type,display_order, configuration)
                    VALUES ('{Guid.NewGuid()}', 'CyberEssentials_SectionTitle', v_form_id, FALSE, FALSE, 3,1, '{{}}')
                    RETURNING id INTO v_section_id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 0, true, 'CyberEssentials_01_Title', 'CyberEssentials_01_Description', '{{}}', NULL, NULL, '_CyberEssentials01', 1)
                    RETURNING id INTO v_question1_id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 3, true, 'CyberEssentials_02_Title', NULL, '{{}}', NULL, 'CyberEssentials_02_SummaryTitle', '_CyberEssentials02', 2)
                    RETURNING id INTO v_question2_id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 3, true, 'CyberEssentials_03_Title', NULL, '{{}}', NULL, 'CyberEssentials_03_SummaryTitle', '_CyberEssentials03', 3)
                    RETURNING id INTO v_question3_id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 2, true, 'CyberEssentials_04_Title', NULL, '{{}}', NULL, 'CyberEssentials_04_SummaryTitle', '_CyberEssentials04', 4)
                    RETURNING id INTO v_question4_id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 7, true, 'CyberEssentials_05_Title', NULL, '{{}}', NULL, 'CyberEssentials_05_SummaryTitle', '_CyberEssentials05', 5)
                    RETURNING id INTO v_question5_id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 3, true, 'CyberEssentials_06_Title', NULL, '{{}}', NULL, 'CyberEssentials_06_SummaryTitle', '_CyberEssentials06', 6)
                    RETURNING id INTO v_question6_id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 3, true, 'CyberEssentials_07_Title', NULL, '{{}}', NULL, 'CyberEssentials_07_SummaryTitle', '_CyberEssentials07', 7)
                    RETURNING id INTO v_question7_id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 2, true, 'CyberEssentials_08_Title', NULL, '{{}}', NULL, 'CyberEssentials_08_SummaryTitle', '_CyberEssentials08', 8)
                    RETURNING id INTO v_question8_id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 7, true, 'CyberEssentials_09_Title', NULL, '{{}}', NULL, 'CyberEssentials_09_SummaryTitle', '_CyberEssentials09', 9)
                    RETURNING id INTO v_question9_id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 6, true, 'Global_CheckYourAnswers', NULL, '{{}}', NULL, NULL, '_CyberEssentials10', 10)
                    RETURNING id INTO v_question10_id;

                    -- Update the next question links
                    UPDATE form_questions SET next_question_id = v_question2_id WHERE title = 'CyberEssentials_01_Title' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question3_id, next_question_alternative_id = v_question10_id WHERE title = 'CyberEssentials_02_Title' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question4_id, next_question_alternative_id = v_question6_id WHERE title = 'CyberEssentials_03_Title' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question5_id WHERE title = 'CyberEssentials_04_Title' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question6_id WHERE title = 'CyberEssentials_05_Title' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question7_id, next_question_alternative_id = v_question10_id WHERE title = 'CyberEssentials_06_Title' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question8_id, next_question_alternative_id = v_question10_id WHERE title = 'CyberEssentials_07_Title' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question9_id WHERE title = 'CyberEssentials_08_Title' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question10_id WHERE title = 'CyberEssentials_09_Title' AND section_id = v_section_id;


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
                    -- Get the ID of the section for Cyber Essentials
                    SELECT id INTO v_section_id FROM form_sections WHERE title = 'CyberEssentials_SectionTitle';

                    IF v_section_id IS NOT NULL THEN
                        -- Delete the questions associated with the section first
                        DELETE FROM form_questions WHERE section_id = v_section_id;
                        -- Delete the Cyber Essentials section
                        DELETE FROM form_sections WHERE id = v_section_id;
                    END IF;
                END $$;
            ");
        }
    }
}

