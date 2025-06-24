using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DataProtectionSectionForm : Migration
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
                BEGIN
                    SELECT id INTO v_form_id FROM forms WHERE name = 'Standard Questions';

                    IF v_form_id IS NULL THEN
                        RAISE EXCEPTION 'Form ""Standard Questions"" not found. Migration cannot proceed.';
                    END IF;

                    INSERT INTO form_sections (guid, title, form_id, allows_multiple_answer_sets, check_further_questions_exempted, type, display_order, configuration)
                    VALUES ('{Guid.NewGuid()}', 'DataProtection_SectionTitle', v_form_id, FALSE, FALSE, 3, 1, '{{}}')
                    RETURNING id INTO v_section_id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"", caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 0, true, 'DataProtection_01_Title', 'DataProtection_01_Description', '{{}}', NULL, '', '_DataProtectionQuestion01', 1)
                    RETURNING id INTO v_question1_id;
                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"", caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 3, true, 'DataProtection_02_Title', 'DataProtection_02_Description', '{{}}', NULL, 'DataProtection_02_SummaryTitle', '_DataProtectionQuestion02', 2)
                    RETURNING id INTO v_question2_id;
                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"", caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 3, true, 'DataProtection_03_Title', 'DataProtection_03_Description', '{{}}', NULL, 'DataProtection_03_SummaryTitle', '_DataProtectionQuestion03', 3)
                    RETURNING id INTO v_question3_id;
                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"", caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 10, true, 'DataProtection_04_Title', 'DataProtection_04_Description', '{{}}', NULL, 'DataProtection_04_SummaryTitle', '_DataProtectionQuestion04', 4)
                    RETURNING id INTO v_question4_id;
                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"", caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 2, false, 'DataProtection_05_Title', NULL, '{{}}', NULL, 'DataProtection_05_SummaryTitle', '_DataProtectionQuestion05', 5)
                    RETURNING id INTO v_question5_id;
                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"", caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 6, true, 'Global_CheckYourAnswers', NULL, '{{}}', NULL, '', '_DataProtectionQuestion06', 6)
                    RETURNING id INTO v_question6_id;

                    -- Update the next question links
                    UPDATE form_questions SET next_question_id = v_question2_id WHERE title = 'DataProtection_01_Title' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question3_id, next_question_alternative_id = v_question4_id WHERE title = 'DataProtection_02_Title' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question4_id WHERE title = 'DataProtection_03_Title' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question5_id WHERE title = 'DataProtection_04_Title' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question6_id, next_question_alternative_id = v_question6_id WHERE title = 'DataProtection_05_Title' AND section_id = v_section_id;
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
                    -- Get the ID of the section for Data Protection
                    SELECT id INTO v_section_id FROM form_sections WHERE title = 'DataProtection_SectionTitle';

                    IF v_section_id IS NOT NULL THEN
                        -- Delete the questions associated with the section first
                        DELETE FROM form_questions WHERE section_id = v_section_id;
                        -- Delete the Data Protection section
                        DELETE FROM form_sections WHERE id = v_section_id;
                    END IF;
                END $$;
            ");
        }
    }
}