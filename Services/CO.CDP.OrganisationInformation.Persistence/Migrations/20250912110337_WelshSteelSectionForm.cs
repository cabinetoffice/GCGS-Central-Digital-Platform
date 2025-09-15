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
                    previousQuestionId INT;
                BEGIN
                    SELECT id INTO form_id FROM forms WHERE name = 'Standard Questions';

	                INSERT INTO form_sections (guid, title, form_id, allows_multiple_answer_sets, check_further_questions_exempted, type, display_order, configuration)
                    VALUES ('{Guid.NewGuid()}', 'WelshSteel_SectionTitle', form_id, FALSE, FALSE, 4, 2, '{{}}');

                    SELECT id INTO sectionId FROM form_sections where title = 'WelshSteel_SectionTitle';

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, sectionId, 6, true, 'Global_CheckYourAnswers', '', '{{}}', NULL, NULL, '_WelshSteel07', 7)
                    RETURNING id INTO previousQuestionId;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', previousQuestionId, NULL, sectionId, 3, false, 'WelshSteel_06_Title', 'WelshSteel_06_Description', '{{}}', NULL, 'WelshSteel_06_SummaryTitle', '_WelshSteel06', 6)
                    RETURNING id INTO previousQuestionId;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', previousQuestionId, NULL, sectionId, 3, false, 'WelshSteel_05_Title', 'WelshSteel_05_Description', '{{}}', NULL, 'WelshSteel_05_SummaryTitle', '_WelshSteel05', 5)
                    RETURNING id INTO previousQuestionId;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', previousQuestionId, NULL, sectionId, 3, false, 'WelshSteel_04_Title', 'WelshSteel_04_Description', '{{}}', NULL, 'WelshSteel_04_SummaryTitle', '_WelshSteel04', 4)
                    RETURNING id INTO previousQuestionId;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', previousQuestionId, NULL, sectionId, 10, false, 'WelshSteel_03_Title', 'WelshSteel_03_Description', '{{}}', NULL, 'WelshSteel_03_SummaryTitle', '_WelshSteel03', 3)
                    RETURNING id INTO previousQuestionId;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', previousQuestionId, NULL, sectionId, 10, false, 'WelshSteel_02_Title', 'WelshSteel_02_Description', '{{}}', NULL, 'WelshSteel_02_SummaryTitle', '_WelshSteel02', 2)
                    RETURNING id INTO previousQuestionId;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', previousQuestionId, NULL, sectionId, 0, true, 'WelshSteel_01_Title', 'WelshSteel_01_Description', '{{}}', NULL, '', '_WelshSteelQuestion01', 1)
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
