using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateWelshEnvironmentalQuestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = $@"
                DO $$
                DECLARE
                    section_id_val INT;
                    cya_id INT;
                BEGIN
                    SELECT id INTO section_id_val FROM form_sections WHERE title = 'Environmental_SectionTitle';
                    SELECT id INTO cya_id FROM form_questions WHERE title = 'Global_CheckYourAnswers' AND section_id = section_id_val;

                    -- update form questions content
                    UPDATE form_questions SET type = 3 WHERE title = 'EnvironmentalQuestion_02_Title';
                    UPDATE form_questions SET type = 2, description = NULL, options = '{{""layout"": {{""button"" : {{""beforeButtonContent"": ""EnvironmentalQuestion_03_CustomInsetText""}}}}}}' WHERE title = 'EnvironmentalQuestion_03_Title';
                    UPDATE form_questions SET type = 3, options = '{{}}' WHERE title = 'EnvironmentalQuestion_04_Title';
                    UPDATE form_questions SET type = 2, options = '{{""layout"": {{""button"" : {{""beforeButtonContent"": ""EnvironmentalQuestion_05_CustomInsetText""}}}}}}' WHERE title = 'EnvironmentalQuestion_05_Title';

                    -- update alternate question navigation
                    UPDATE form_questions SET next_question_alternative_id = (SELECT id FROM form_questions WHERE title = 'EnvironmentalQuestion_04_Title') WHERE title = 'EnvironmentalQuestion_02_Title';
                    UPDATE form_questions SET next_question_alternative_id = cya_id WHERE title = 'EnvironmentalQuestion_04_Title';

                END $$;
            ";

            migrationBuilder.Sql(sql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var sql = $@"
                DO $$
                BEGIN
                    -- revert form question content updates made in Up()
                    UPDATE form_questions SET type = 2 WHERE title = 'EnvironmentalQuestion_02_Title';
                    UPDATE form_questions SET type = 10, description = 'EnvironmentalQuestion_03_Description', options = '{{}}' WHERE title = 'EnvironmentalQuestion_03_Title';
                    UPDATE form_questions SET type = 2, options = '{{""layout"": {{""button"" : {{""beforeButtonContent"": ""EnvironmentalQuestion_04_CustomInsetText""}}}}}}' WHERE title = 'EnvironmentalQuestion_04_Title';
                    UPDATE form_questions SET type = 10, options = '{{}}' WHERE title = 'EnvironmentalQuestion_05_Title';

                    -- reset navigation alternate linking to previous state
                    UPDATE form_questions SET next_question_alternative_id = NULL WHERE title = 'EnvironmentalQuestion_02_Title';
                    UPDATE form_questions SET next_question_alternative_id = NULL WHERE title = 'EnvironmentalQuestion_04_Title';

                END $$;
            ";

            migrationBuilder.Sql(sql);
        }
    }
}
