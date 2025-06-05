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

                 BEGIN
                     -- Get the ID of the form named 'Standard Questions'
                     SELECT id INTO form_id FROM forms WHERE name = 'Standard Questions';

                     -- Insert a new section for Modern Slavery
                     INSERT INTO form_sections (guid, title, form_id, allows_multiple_answer_sets, check_further_questions_exempted, type, display_order, configuration)
                     VALUES ('{Guid.NewGuid()}', 'ModernSlavery_SectionTitle', form_id, FALSE, FALSE, 3, 1, '{{}}')
                     RETURNING id INTO sectionId;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', previousQuestionId, NULL, sectionId, 3, true, 'ModernSlavery_02_Title', 'ModernSlavery_02_Description', '{{}}', NULL, '', '_ModernSlaveryQuestion02', 2)
                    RETURNING id INTO previousQuestionId;

                     -- Insert the first question for Modern Slavery
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
