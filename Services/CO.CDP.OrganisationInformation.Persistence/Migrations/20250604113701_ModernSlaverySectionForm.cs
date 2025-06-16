using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable
namespace CO.CDP.OrganisationInformation.Persistence.Migrations {
/// <inheritdoc />
public partial class ModernSlaverySectionForm : Migration
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
                    v_question11_id INTEGER;
                    v_question12_id INTEGER;
                    v_question13_id INTEGER;
                BEGIN
                    SELECT id INTO v_form_id FROM forms WHERE name = 'Standard Questions';

                    IF v_form_id IS NULL THEN
                        RAISE EXCEPTION 'Form ""Standard Questions"" not found. Migration cannot proceed.';
                    END IF;

                    INSERT INTO form_sections (guid, title, form_id, allows_multiple_answer_sets, check_further_questions_exempted, type, display_order, configuration)
                    VALUES ('{Guid.NewGuid()}', 'ModernSlavery_SectionTitle', v_form_id, FALSE, FALSE, 3, 1, '{{}}')
                    RETURNING id INTO v_section_id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"", caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 0, true, 'ModernSlavery_01_Title', 'ModernSlavery_01_Description', '{{}}', NULL, '', '_ModernSlaveryQuestion01', 1)
                    RETURNING id INTO v_question1_id;
                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"", caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 3, true, 'ModernSlavery_02_Title', 'ModernSlavery_02_Description', '{{}}', NULL, 'ModernSlavery_02_SummaryTitle', '_ModernSlaveryQuestion02', 2)
                    RETURNING id INTO v_question2_id;
                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"", caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 3, true, 'ModernSlavery_03_Title', '', '{{}}', NULL, 'ModernSlavery_03_SummaryTitle', '_ModernSlaveryQuestion03', 3)
                    RETURNING id INTO v_question3_id;
                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"", caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 3, true, 'ModernSlavery_04_Title', 'ModernSlavery_04_Description', '{{}}', NULL, 'ModernSlavery_04_SummaryTitle', '_ModernSlaveryQuestion04', 4)
                    RETURNING id INTO v_question4_id;
                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"", caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 12, true, 'ModernSlavery_05_Title', 'ModernSlavery_05_Description', '{{}}', NULL, 'ModernSlavery_05_SummaryTitle', '_ModernSlaveryQuestion05', 5)
                    RETURNING id INTO v_question5_id;
                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"", caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 2, true, 'ModernSlavery_06_Title', '', '{{}}', NULL, 'ModernSlavery_06_SummaryTitle', '_ModernSlaveryQuestion06', 6)
                    RETURNING id INTO v_question6_id;
                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"", caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 12, true, 'ModernSlavery_07_Title', NULL, '{{}}', NULL, 'ModernSlavery_07_SummaryTitle', '_ModernSlaveryQuestion07', 7)
                    RETURNING id INTO v_question7_id;
                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"", caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 2, true, 'ModernSlavery_08_Title', NULL, '{{}}', NULL, 'ModernSlavery_08_SummaryTitle', '_ModernSlaveryQuestion08', 8)
                    RETURNING id INTO v_question8_id;
                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"", caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 10, true, 'ModernSlavery_09_Title', 'ModernSlavery_09_Description', '{{}}', NULL, 'ModernSlavery_09_SummaryTitle', '_ModernSlaveryQuestion09', 9)
                    RETURNING id INTO v_question9_id;
                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"", caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 10, true, 'ModernSlavery_10_Title', 'ModernSlavery_10_Description', '{{}}', NULL, 'ModernSlavery_10_SummaryTitle', '_ModernSlaveryQuestion10', 10)
                    RETURNING id INTO v_question10_id;
                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"", caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 10, true, 'ModernSlavery_11_Title', 'ModernSlavery_11_Description', '{{}}', NULL, 'ModernSlavery_11_SummaryTitle', '_ModernSlaveryQuestion11', 11)
                    RETURNING id INTO v_question11_id;
                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"", caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 10, true, 'ModernSlavery_12_Title', 'ModernSlavery_12_Description', '{{}}', NULL, 'ModernSlavery_12_SummaryTitle', '_ModernSlaveryQuestion12', 12)
                    RETURNING id INTO v_question12_id;
                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"", caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 6, true, 'Global_CheckYourAnswers', NULL, '{{}}', NULL, '', '_ModernSlaveryQuestion13', 13)
                    RETURNING id INTO v_question13_id;

                    -- Update the next question links
                    UPDATE form_questions SET next_question_id = v_question2_id WHERE title = 'ModernSlavery_01_Title' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question3_id, next_question_alternative_id = v_question4_id WHERE title = 'ModernSlavery_02_Title' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question5_id, next_question_alternative_id = v_question6_id WHERE title = 'ModernSlavery_03_Title' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question7_id, next_question_alternative_id = v_question8_id WHERE title = 'ModernSlavery_04_Title' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question9_id WHERE title = 'ModernSlavery_05_Title' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question9_id WHERE title = 'ModernSlavery_06_Title' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question10_id WHERE title = 'ModernSlavery_07_Title' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question11_id WHERE title = 'ModernSlavery_08_Title' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question12_id WHERE title = 'ModernSlavery_09_Title' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question11_id WHERE title = 'ModernSlavery_10_Title' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question13_id WHERE title = 'ModernSlavery_11_Title' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question13_id WHERE title = 'ModernSlavery_12_Title' AND section_id = v_section_id;
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
                    -- Get the ID of the section for Modern Slavery
                    SELECT id INTO v_section_id FROM form_sections WHERE title = 'ModernSlavery_SectionTitle';

                    IF v_section_id IS NOT NULL THEN
                        -- Delete the questions associated with the section first
                        DELETE FROM form_questions WHERE section_id = v_section_id;
                        -- Delete the Modern Slavery section
                        DELETE FROM form_sections WHERE id = v_section_id;
                    END IF;
                END $$;
            ");
    }
}
}
