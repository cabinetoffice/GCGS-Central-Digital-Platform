using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModernSlaveryQuestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = $@"
                DO $$
                BEGIN
                    -- Update navigation linking for questions 5 & 6, replacing the link to question 9 to question 12
                    UPDATE form_questions SET next_question_id = (SELECT id FROM form_questions WHERE title = 'ModernSlavery_12_Title') WHERE title = 'ModernSlavery_05_Title';
                    UPDATE form_questions SET next_question_id = (SELECT id FROM form_questions WHERE title = 'ModernSlavery_12_Title') WHERE title = 'ModernSlavery_06_Title';

                    -- Remove question 9
                    DELETE FROM form_questions WHERE name = '_ModernSlaveryQuestion09';

                    -- Update navigation linking for question 7, replacing the link to question 10 to question 11
                    UPDATE form_questions SET next_question_id = (SELECT id FROM form_questions WHERE title = 'ModernSlavery_11_Title') WHERE title = 'ModernSlavery_07_Title';

                    -- Remove question 10
                    DELETE FROM form_questions WHERE name = '_ModernSlaveryQuestion10';

                    -- Update question 6 to optional and set inset text
                    UPDATE form_questions SET is_required = false, options = '{{""layout"": {{""button"" : {{""beforeButtonContent"": ""ModernSlavery_06_CustomInsetText""}}}}}}' WHERE title = 'ModernSlavery_06_Title';

                    -- Update question 8 to optional and set inset text
                    UPDATE form_questions SET is_required = false, options = '{{""layout"": {{""button"" : {{""beforeButtonContent"": ""ModernSlavery_08_CustomInsetText""}}}}}}'  WHERE title = 'ModernSlavery_08_Title';

                END $$;
            ";

            migrationBuilder.Sql(sql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var sql = $@"
                DO $$
                DECLARE
                    section_id_val INT;
                    q9_id INT;
                    q10_id INT;
                BEGIN
                    SELECT id INTO section_id_val FROM form_sections WHERE title = 'ModernSlavery_SectionTitle';

                    -- Re-insert question 9
                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"", caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, section_id_val, 10, true, 'ModernSlavery_09_Title', 'ModernSlavery_09_Description', '{{}}', NULL, 'ModernSlavery_09_SummaryTitle', '_ModernSlaveryQuestion09', 9)
                    RETURNING id INTO q9_id;

                    -- Update navigation linking for question 5 & 6 to link to question 9
                    UPDATE form_questions SET next_question_id = q9_id WHERE title = 'ModernSlavery_05_Title';
                    UPDATE form_questions SET next_question_id = q9_id WHERE title = 'ModernSlavery_06_Title';

                    -- Update navigation linking for question 9 to link to question 12
                    UPDATE form_questions SET next_question_id = (SELECT id FROM form_questions WHERE title = 'ModernSlavery_12_Title') WHERE title = 'ModernSlavery_09_Title';

                    -- Re-insert question 10
                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"", caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, section_id_val, 10, true, 'ModernSlavery_10_Title', 'ModernSlavery_10_Description', '{{}}', NULL, 'ModernSlavery_10_SummaryTitle', '_ModernSlaveryQuestion10', 10)
                    RETURNING id INTO q10_id;

                    -- Update navigation linking for question 7 to link to question 10
                    UPDATE form_questions SET next_question_id = q10_id WHERE title = 'ModernSlavery_07_Title';

                    -- Update navigation linking for question 10 to link to question 11
                    UPDATE form_questions SET next_question_id = (SELECT id FROM form_questions WHERE title = 'ModernSlavery_11_Title') WHERE title = 'ModernSlavery_10_Title';

                    -- Update question 6 to required and remove inset text
                    UPDATE form_questions SET is_required = true, options = '{{}}' WHERE title = 'ModernSlavery_06_Title';

                    -- Update question 8 to required and remove inset text
                    UPDATE form_questions SET is_required = true, options = '{{}}' WHERE title = 'ModernSlavery_08_Title';

                END $$;
            ";

            migrationBuilder.Sql(sql);
        }
    }
}
