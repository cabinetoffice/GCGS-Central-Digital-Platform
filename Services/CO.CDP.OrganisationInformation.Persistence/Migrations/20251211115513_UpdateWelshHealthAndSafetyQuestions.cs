using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateWelshHealthAndSafetyQuestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var healthAndSafety06GroupId = new Guid("12345678-1234-1234-1234-123456789006"); // Group 6 summary group
            var healthAndSafety07GroupId = new Guid("12345678-1234-1234-1234-123456789007"); // Group 7 summary group

            var healthAndSafety06GroupSummaryTitle = "WelshHealthAndSafetyQuestion_Group6_SummaryTitle";
            var healthAndSafety07GroupSummaryTitle = "WelshHealthAndSafetyQuestion_Group7_SummaryTitle";

            // Base JSON for grouping structures
            var healthAndSafety06GroupingJsonFragment = $"\"grouping\": {{ \"id\": \"{healthAndSafety06GroupId}\", \"page\": false, \"checkYourAnswers\": true, \"summaryTitle\": \"{healthAndSafety06GroupSummaryTitle}\" }}";
            var healthAndSafety07GroupingJsonFragment = $"\"grouping\": {{ \"id\": \"{healthAndSafety07GroupId}\", \"page\": false, \"checkYourAnswers\": true, \"summaryTitle\": \"{healthAndSafety07GroupSummaryTitle}\" }}";

            // JSON for grouped questions
            var healthAndSafety06GroupedOptionsJson = $"'{{ {healthAndSafety06GroupingJsonFragment} }}'";
            var healthAndSafety07GroupedOptionsJson = $"'{{ {healthAndSafety07GroupingJsonFragment} }}'";

            // JSON for HealthAndSafety_11_1 question options with inset text
            var healthAndSafety11_1_OptionsWithInsetTextJson = $"'{{\"layout\": {{\"button\" : {{\"beforeButtonContent\": \"WelshHealthAndSafetyQuestion_11_01_CustomInsetText\"}}}}, {healthAndSafety07GroupingJsonFragment} }}'";

            // JSON for HealthAndSafety_10 question options with inset text
            var healthAndSafety10_OptionsWithInsetTextJson = $"'{{\"layout\": {{\"button\" : {{\"beforeButtonContent\": \"WelshHealthAndSafetyQuestion_10_CustomInsetText\"}}}}, {healthAndSafety07GroupingJsonFragment} }}'";

            var sql = $@"
                DO $$
                DECLARE
                    section_id_val INT;
                    cya_id INT;
                    q8_01_Id INT;
                    q10_01_Id INT;
                    q11_01_Id INT;
                BEGIN
                    SELECT id INTO section_id_val FROM form_sections WHERE title = 'WelshHealthAndSafety_SectionTitle';
                    SELECT id INTO cya_id FROM form_questions WHERE title = 'Global_CheckYourAnswers' AND section_id = section_id_val;

                    -- Insert new questions
                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, section_id_val, 3, true, 'WelshHealthAndSafetyQuestion_08_01_Title', 'WelshHealthAndSafetyQuestion_08_01_Description', " + healthAndSafety06GroupedOptionsJson + $@", NULL, 'WelshHealthAndSafetyQuestion_08_01_SummaryTitle', '_WelshHealthAndSafetyQuestion08_1', 14)
                    RETURNING id INTO q8_01_Id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, section_id_val, 3, true, 'WelshHealthAndSafetyQuestion_10_01_Title', 'WelshHealthAndSafetyQuestion_10_01_Description', " + healthAndSafety07GroupedOptionsJson + $@", NULL, 'WelshHealthAndSafetyQuestion_10_01_SummaryTitle', '_WelshHealthAndSafetyQuestion10_1', 17)
                    RETURNING id INTO q10_01_Id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, section_id_val, 3, true, 'WelshHealthAndSafetyQuestion_11_01_Title', NULL, " + healthAndSafety11_1_OptionsWithInsetTextJson + $@", NULL, 'WelshHealthAndSafetyQuestion_11_01_SummaryTitle', '_WelshHealthAndSafetyQuestion11_1', 19)
                    RETURNING id INTO q11_01_Id;

                    -- update question navigation
                    -- update navigation linking for q8_01_Id
                    UPDATE form_questions SET next_question_id = q8_01_Id WHERE title = 'WelshHealthAndSafetyQuestion_07_03_Title';
                    UPDATE form_questions SET next_question_alternative_id = q8_01_Id WHERE title = 'WelshHealthAndSafetyQuestion_06_Title';
                    UPDATE form_questions SET next_question_id = (SELECT id FROM form_questions WHERE title = 'WelshHealthAndSafetyQuestion_08_Title'), next_question_alternative_id = (SELECT id FROM form_questions WHERE title = 'WelshHealthAndSafetyQuestion_09_Title') WHERE id = q8_01_Id;

                    -- update navigation linking for q10_01_Id
                    UPDATE form_questions SET next_question_id = q10_01_Id WHERE title = 'WelshHealthAndSafetyQuestion_09_Title';
                    UPDATE form_questions SET next_question_id = (SELECT id FROM form_questions WHERE title = 'WelshHealthAndSafetyQuestion_10_Title'), next_question_alternative_id = q11_01_Id WHERE id = q10_01_Id;

                    -- update navigation linking for q11_01_Id
                    UPDATE form_questions SET next_question_id = q11_01_Id WHERE title = 'WelshHealthAndSafetyQuestion_10_Title';
                    UPDATE form_questions SET next_question_id = (SELECT id FROM form_questions WHERE title = 'WelshHealthAndSafetyQuestion_11_Title'), next_question_alternative_id = cya_id WHERE id = q11_01_Id;

                    -- content updates
                    UPDATE form_questions SET description = NULL WHERE title = 'WelshHealthAndSafetyQuestion_02_Title';
                    UPDATE form_questions SET description = NULL WHERE title = 'WelshHealthAndSafetyQuestion_08_Title';
                    UPDATE form_questions SET options = {healthAndSafety10_OptionsWithInsetTextJson}, description = NULL WHERE title = 'WelshHealthAndSafetyQuestion_10_Title';

                    -- update sort orders
                    UPDATE form_questions SET sort_order = 15 WHERE title = 'WelshHealthAndSafetyQuestion_08_Title';
                    UPDATE form_questions SET sort_order = 16 WHERE title = 'WelshHealthAndSafetyQuestion_09_Title';
                    UPDATE form_questions SET sort_order = 18 WHERE title = 'WelshHealthAndSafetyQuestion_10_Title';
                    UPDATE form_questions SET sort_order = 20 WHERE title = 'WelshHealthAndSafetyQuestion_11_Title';
                    UPDATE form_questions SET sort_order = 21 WHERE id = cya_id;

                END $$;
            ";

            migrationBuilder.Sql(sql);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var healthAndSafety07GroupId = new Guid("12345678-1234-1234-1234-123456789007"); // Group 7 summary group

            var healthAndSafety07GroupSummaryTitle = "WelshHealthAndSafetyQuestion_Group7_SummaryTitle";

            // Base JSON for grouping structures
            var healthAndSafety07GroupingJsonFragment = $"\"grouping\": {{ \"id\": \"{healthAndSafety07GroupId}\", \"page\": false, \"checkYourAnswers\": true, \"summaryTitle\": \"{healthAndSafety07GroupSummaryTitle}\" }}";

            // JSON for grouped questions
            var healthAndSafety07GroupedOptionsJson = $"'{{ {healthAndSafety07GroupingJsonFragment} }}'";

            var sql = $@"
                DO $$
                DECLARE
                    section_id_val INT;
                    cya_id INT;
                BEGIN
                    SELECT id INTO section_id_val FROM form_sections WHERE title = 'WelshHealthAndSafety_SectionTitle';
                    SELECT id INTO cya_id FROM form_questions WHERE title = 'Global_CheckYourAnswers' AND section_id = section_id_val;

                    -- reset navigation linking to previous state
                    UPDATE form_questions SET next_question_id = (SELECT id FROM form_questions WHERE title = 'WelshHealthAndSafetyQuestion_08_Title') WHERE title = 'WelshHealthAndSafetyQuestion_07_03_Title';
                    UPDATE form_questions SET next_question_alternative_id = (SELECT id FROM form_questions WHERE title = 'WelshHealthAndSafetyQuestion_08_Title') WHERE title = 'WelshHealthAndSafetyQuestion_06_Title';
                    UPDATE form_questions SET next_question_id = (SELECT id FROM form_questions WHERE title = 'WelshHealthAndSafetyQuestion_10_Title') WHERE title = 'WelshHealthAndSafetyQuestion_09_Title';
                    UPDATE form_questions SET next_question_id = (SELECT id FROM form_questions WHERE title = 'WelshHealthAndSafetyQuestion_11_Title') WHERE title = 'WelshHealthAndSafetyQuestion_10_Title';

                    --revert content updates
                    UPDATE form_questions SET description = 'WelshHealthAndSafetyQuestion_02_Description' WHERE title = 'WelshHealthAndSafetyQuestion_02_Title';
                    UPDATE form_questions SET description = 'WelshHealthAndSafetyQuestion_08_Description' WHERE title = 'WelshHealthAndSafetyQuestion_08_Title';
                    UPDATE form_questions SET description = 'WelshHealthAndSafetyQuestion_10_Description' WHERE title = 'WelshHealthAndSafetyQuestion_10_Title';
                    UPDATE form_questions SET options = {healthAndSafety07GroupedOptionsJson}, description = 'WelshHealthAndSafetyQuestion_10_Description' WHERE title = 'WelshHealthAndSafetyQuestion_10_Title';

                    -- revert sort orders
                    UPDATE form_questions SET sort_order = 14 WHERE title = 'WelshHealthAndSafetyQuestion_08_Title';
                    UPDATE form_questions SET sort_order = 15 WHERE title = 'WelshHealthAndSafetyQuestion_09_Title';
                    UPDATE form_questions SET sort_order = 16 WHERE title = 'WelshHealthAndSafetyQuestion_10_Title';
                    UPDATE form_questions SET sort_order = 17 WHERE title = 'WelshHealthAndSafetyQuestion_11_Title';
                    UPDATE form_questions SET sort_order = 18 WHERE id = cya_id;

                    -- remove the questions that were added in the Up() method
                    DELETE FROM form_questions WHERE name IN (
                        '_WelshHealthAndSafetyQuestion08_1', '_WelshHealthAndSafetyQuestion10_1',
                        '_WelshHealthAndSafetyQuestion11_1'
                    );

                END $$;
            ";
            migrationBuilder.Sql(sql);
        }
    }
}
