using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class WelshSteelSectionForm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
migrationBuilder.Sql($@"
        DO $$
                DECLARE
                    form_id int;
                    sectionId INT;
                    q1Id INT;
                    q2Id INT;
                    q3Id INT;
                    q4Id INT;
                    q5Id INT;
                    q6Id INT;
                    q7Id INT;
                    q8Id INT;
                    q9Id INT;
                BEGIN
                    SELECT id INTO form_id FROM forms WHERE name = 'Standard Questions';

	                INSERT INTO form_sections (guid, title, form_id, allows_multiple_answer_sets, check_further_questions_exempted, type, display_order, configuration)
                    VALUES ('{Guid.NewGuid()}', 'WelshSteel_SectionTitle', form_id, FALSE, FALSE, 4, 4, '{{}}')
                    RETURNING id INTO sectionId;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, sectionId, 0, false, 'WelshSteel_01_Title', 'WelshSteel_01_Description', '{{}}', NULL, 'WelshSteel_01_SummaryTitle', '_WelshSteel01', 1)
                    RETURNING id INTO q1Id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, sectionId, 2, false, 'WelshSteel_02_Title', 'WelshSteel_02_Description', '{{}}', NULL, 'WelshSteel_02_SummaryTitle', '_WelshSteel02', 2)
                    RETURNING id INTO q2Id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, sectionId, 10, true, 'WelshSteel_03_Title', 'WelshSteel_03_Description', '{{}}', NULL, 'WelshSteel_03_SummaryTitle', '_WelshSteel03', 3)
                    RETURNING id INTO q3Id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, sectionId, 2, false, 'WelshSteel_04_Title', 'WelshSteel_04_Description', '{{}}', NULL, 'WelshSteel_04_SummaryTitle', '_WelshSteel04', 4)
                    RETURNING id INTO q4Id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, sectionId, 10, true, 'WelshSteel_05_Title', 'WelshSteel_05_Description', '{{}}', NULL, 'WelshSteel_05_SummaryTitle', '_WelshSteel05', 5)
                    RETURNING id INTO q5Id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, sectionId, 10, false, 'WelshSteel_06_Title', 'WelshSteel_06_Description', '{{}}', NULL, 'WelshSteel_06_SummaryTitle', '_WelshSteel06', 6)
                    RETURNING id INTO q6Id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, sectionId, 10, false, 'WelshSteel_07_Title', 'WelshSteel_07_Description', '{{}}', NULL, 'WelshSteel_07_SummaryTitle', '_WelshSteel07', 7)
                    RETURNING id INTO q7Id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, sectionId, 10, false, 'WelshSteel_08_Title', 'WelshSteel_08_Description', '{{}}', NULL, 'WelshSteel_08_SummaryTitle', '_WelshSteel08', 8)
                    RETURNING id INTO q8Id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, sectionId, 6, true, 'Global_CheckYourAnswers', '', '{{}}', NULL, NULL, '_WelshSteel09', 9)
                    RETURNING id INTO q9Id;

                    UPDATE form_questions SET next_question_id = q2Id WHERE id = q1Id;
                    UPDATE form_questions SET next_question_id = q4Id, next_question_alternative_id = q3Id WHERE id = q2Id;
                    UPDATE form_questions SET next_question_id = q4Id WHERE id = q3Id;
                    UPDATE form_questions SET next_question_id = q6Id, next_question_alternative_id = q5Id WHERE id = q4Id;
                    UPDATE form_questions SET next_question_id = q6Id WHERE id = q5Id;
                    UPDATE form_questions SET next_question_id = q7Id WHERE id = q6Id;
                    UPDATE form_questions SET next_question_id = q8Id WHERE id = q7Id;
                    UPDATE form_questions SET next_question_id = q9Id WHERE id = q8Id;
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
                    SELECT id INTO v_section_id FROM form_sections WHERE title = 'WelshSteel_SectionTitle';

                    IF v_section_id IS NOT NULL THEN
                        DELETE FROM form_questions WHERE section_id = v_section_id;
                        DELETE FROM form_sections WHERE id = v_section_id;
                    END IF;
                END $$;
            ");
        }
    }
}
