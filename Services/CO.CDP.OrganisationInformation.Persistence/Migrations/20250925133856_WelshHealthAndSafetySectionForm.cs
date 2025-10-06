using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class WelshHealthAndSafetySectionForm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var healthAndSafety01GroupId = new Guid("12345678-1234-1234-1234-123456789001"); // Group 1: HealthAndSafety_02-03
            var healthAndSafety02GroupId = new Guid("12345678-1234-1234-1234-123456789002"); // Group 2: HealthAndSafety_04
            var healthAndSafety03GroupId = new Guid("12345678-1234-1234-1234-123456789003"); // Group 3: HealthAndSafety_05
            var healthAndSafety04GroupId = new Guid("12345678-1234-1234-1234-123456789004"); // Group 4: HealthAndSafety_06
            var healthAndSafety05GroupId = new Guid("12345678-1234-1234-1234-123456789005"); // Group 5: HealthAndSafety_07
            var healthAndSafety06GroupId = new Guid("12345678-1234-1234-1234-123456789006"); // Group 6: HealthAndSafety_08-09
            var healthAndSafety07GroupId = new Guid("12345678-1234-1234-1234-123456789007"); // Group 7: HealthAndSafety_10-11

            var healthAndSafety01GroupSummaryTitle = "WelshHealthAndSafetyQuestion_Group1_SummaryTitle";
            var healthAndSafety02GroupSummaryTitle = "WelshHealthAndSafetyQuestion_Group2_SummaryTitle";
            var healthAndSafety03GroupSummaryTitle = "WelshHealthAndSafetyQuestion_Group3_SummaryTitle";
            var healthAndSafety04GroupSummaryTitle = "WelshHealthAndSafetyQuestion_Group4_SummaryTitle";
            var healthAndSafety05GroupSummaryTitle = "WelshHealthAndSafetyQuestion_Group5_SummaryTitle";
            var healthAndSafety06GroupSummaryTitle = "WelshHealthAndSafetyQuestion_Group6_SummaryTitle";
            var healthAndSafety07GroupSummaryTitle = "WelshHealthAndSafetyQuestion_Group7_SummaryTitle";

            // Base JSON for grouping structures
            var healthAndSafety01GroupingJsonFragment = $"\"grouping\": {{ \"id\": \"{healthAndSafety01GroupId}\", \"page\": false, \"checkYourAnswers\": true, \"summaryTitle\": \"{healthAndSafety01GroupSummaryTitle}\" }}";
            var healthAndSafety02GroupingSingleQuestionJsonFragment = $"\"grouping\": {{ \"id\": \"{healthAndSafety02GroupId}\", \"page\": false, \"checkYourAnswers\": true, \"summaryTitle\": \"{healthAndSafety02GroupSummaryTitle}\" }}";
            var healthAndSafety03GroupingMultiQuestionJsonFragment = $"\"grouping\": {{ \"id\": \"{healthAndSafety03GroupId}\", \"page\": true, \"checkYourAnswers\": true, \"summaryTitle\": \"{healthAndSafety03GroupSummaryTitle}\" }}";
            var healthAndSafety04GroupingSingleQuestionJsonFragment = $"\"grouping\": {{ \"id\": \"{healthAndSafety04GroupId}\", \"page\": false, \"checkYourAnswers\": true, \"summaryTitle\": \"{healthAndSafety04GroupSummaryTitle}\" }}";
            var healthAndSafety05GroupingMultiQuestionJsonFragment = $"\"grouping\": {{ \"id\": \"{healthAndSafety05GroupId}\", \"page\": true, \"checkYourAnswers\": true, \"summaryTitle\": \"{healthAndSafety05GroupSummaryTitle}\" }}";
            var healthAndSafety06GroupingJsonFragment = $"\"grouping\": {{ \"id\": \"{healthAndSafety06GroupId}\", \"page\": false, \"checkYourAnswers\": true, \"summaryTitle\": \"{healthAndSafety06GroupSummaryTitle}\" }}";
            var healthAndSafety07GroupingJsonFragment = $"\"grouping\": {{ \"id\": \"{healthAndSafety07GroupId}\", \"page\": false, \"checkYourAnswers\": true, \"summaryTitle\": \"{healthAndSafety07GroupSummaryTitle}\" }}";

            // JSON for regular grouped questions
            var healthAndSafety01GroupedOptionsJson = $"'{{ {healthAndSafety01GroupingJsonFragment} }}'";
            var healthAndSafety02SingleQuestionGroupedOptionsJson = $"'{{ {healthAndSafety02GroupingSingleQuestionJsonFragment} }}'";
            var healthAndSafety03MultiQuestionGroupedOptionsJson = $"'{{ {healthAndSafety03GroupingMultiQuestionJsonFragment} }}'";
            var healthAndSafety04SingleQuestionGroupedOptionsJson  = $"'{{ {healthAndSafety04GroupingSingleQuestionJsonFragment} }}'";
            var healthAndSafety05MultiQuestionGroupedOptionsJson = $"'{{ {healthAndSafety05GroupingMultiQuestionJsonFragment} }}'";
            var healthAndSafety06GroupedOptionsJson = $"'{{ {healthAndSafety06GroupingJsonFragment} }}'";
            var healthAndSafety07GroupedOptionsJson = $"'{{ {healthAndSafety07GroupingJsonFragment} }}'";

            // JSON for HealthAndSafety_08 question options with inset text
            var healthAndSafety08OptionsWithInsetTextJson = $"'{{\"layout\": {{\"button\" : {{\"beforeButtonContent\": \"WelshHealthAndSafetyQuestion_08_CustomInsetText\"}}}}, {healthAndSafety06GroupingJsonFragment} }}'";

            // JSON for HealthAndSafety_11 question options with inset text
            var healthAndSafety11OptionsWithInsetTextJson = $"'{{\"layout\": {{\"button\" : {{\"beforeButtonContent\": \"WelshHealthAndSafetyQuestion_11_CustomInsetText\"}}}}, {healthAndSafety07GroupingJsonFragment} }}'";

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
                    q5Id_1_Id INT;
                    q5Id_2_Id INT;
                    q5Id_3_Id INT;
                    q6Id INT;
                    q7Id INT;
                    q7Id_1_Id INT;
                    q7Id_2_Id INT;
                    q7Id_3_Id INT;
                    q8Id INT;
                    q9Id INT;
                    q10Id INT;
                    q11Id INT;
                    q12Id INT;
                BEGIN
                    SELECT id INTO form_id FROM forms WHERE name = 'Standard Questions';

	                INSERT INTO form_sections (guid, title, form_id, allows_multiple_answer_sets, check_further_questions_exempted, type, display_order, configuration)
                    VALUES ('{Guid.NewGuid()}', 'WelshHealthAndSafety_SectionTitle', form_id, FALSE, FALSE, 4, 4, '{{}}')
                    RETURNING id INTO sectionId;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, sectionId, 0, true, 'WelshHealthAndSafetyQuestion_01_Title', 'WelshHealthAndSafetyQuestion_01_Description', '{{""layout"": {{""button"": {{""style"": ""Start"", ""text"": ""Global_Start""}}}}}}', NULL, 'WelshHealthAndSafetyQuestion_01_SummaryTitle', '_WelshHealthAndSafetyQuestion01', 1)
                    RETURNING id INTO q1Id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, sectionId, 3, true, 'WelshHealthAndSafetyQuestion_02_Title', NULL, " + healthAndSafety01GroupedOptionsJson + $@", NULL, 'WelshHealthAndSafetyQuestion_02_SummaryTitle', '_WelshHealthAndSafetyQuestion02', 2)
                    RETURNING id INTO q2Id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, sectionId, 10, true, 'WelshHealthAndSafetyQuestion_03_Title', 'WelshHealthAndSafetyQuestion_03_Description', " + healthAndSafety01GroupedOptionsJson + $@", NULL, 'WelshHealthAndSafetyQuestion_03_SummaryTitle', '_WelshHealthAndSafetyQuestion03', 3)
                    RETURNING id INTO q3Id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, sectionId, 3, true, 'WelshHealthAndSafetyQuestion_04_Title', 'WelshHealthAndSafetyQuestion_04_Description', " + healthAndSafety02SingleQuestionGroupedOptionsJson + $@", NULL, 'WelshHealthAndSafetyQuestion_04_SummaryTitle', '_WelshHealthAndSafetyQuestion04', 4)
                    RETURNING id INTO q4Id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, sectionId, 0, true, 'WelshHealthAndSafetyQuestion_05_Title', NULL, " + healthAndSafety03MultiQuestionGroupedOptionsJson + $@", NULL, '', '_WelshHealthAndSafetyQuestion05', 5)
                    RETURNING id INTO q5Id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, sectionId, 1, true, 'WelshHealthAndSafetyQuestion_05_01_Title', NULL, " + healthAndSafety03MultiQuestionGroupedOptionsJson + $@", NULL, 'WelshHealthAndSafetyQuestion_05_01_SummaryTitle', '_WelshHealthAndSafetyQuestion05_1', 6)
                    RETURNING id INTO q5Id_1_Id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, sectionId, 1, true, 'WelshHealthAndSafetyQuestion_05_02_Title', NULL, " + healthAndSafety03MultiQuestionGroupedOptionsJson + $@", NULL, 'WelshHealthAndSafetyQuestion_05_02_SummaryTitle', '_WelshHealthAndSafetyQuestion05_2', 7)
                    RETURNING id INTO q5Id_2_Id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, sectionId, 10, true, 'WelshHealthAndSafetyQuestion_05_03_Title', NULL, " + healthAndSafety03MultiQuestionGroupedOptionsJson + $@", NULL, 'WelshHealthAndSafetyQuestion_05_03_SummaryTitle', '_WelshHealthAndSafetyQuestion05_3', 8)
                    RETURNING id INTO q5Id_3_Id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, sectionId, 3, true, 'WelshHealthAndSafetyQuestion_06_Title', 'WelshHealthAndSafetyQuestion_06_Description', " + healthAndSafety04SingleQuestionGroupedOptionsJson + $@", NULL, 'WelshHealthAndSafetyQuestion_06_SummaryTitle', '_WelshHealthAndSafetyQuestion06', 9)
                    RETURNING id INTO q6Id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, sectionId, 0, true, 'WelshHealthAndSafetyQuestion_07_Title', NULL, " + healthAndSafety05MultiQuestionGroupedOptionsJson + $@", NULL, '', '_WelshHealthAndSafetyQuestion07', 10)
                    RETURNING id INTO q7Id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, sectionId, 1, true, 'WelshHealthAndSafetyQuestion_07_01_Title', NULL, " + healthAndSafety05MultiQuestionGroupedOptionsJson + $@", NULL, 'WelshHealthAndSafetyQuestion_07_01_SummaryTitle', '_WelshHealthAndSafetyQuestion07_1', 11)
                    RETURNING id INTO q7Id_1_Id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, sectionId, 1, true, 'WelshHealthAndSafetyQuestion_07_02_Title', NULL, " + healthAndSafety05MultiQuestionGroupedOptionsJson + $@", NULL, 'WelshHealthAndSafetyQuestion_07_02_SummaryTitle', '_WelshHealthAndSafetyQuestion07_2', 12)
                    RETURNING id INTO q7Id_2_Id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, sectionId, 10, true, 'WelshHealthAndSafetyQuestion_07_03_Title', NULL, " + healthAndSafety05MultiQuestionGroupedOptionsJson + $@", NULL, 'WelshHealthAndSafetyQuestion_07_03_SummaryTitle', '_WelshHealthAndSafetyQuestion07_3', 13)
                    RETURNING id INTO q7Id_3_Id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, sectionId, 2, false, 'WelshHealthAndSafetyQuestion_08_Title', 'WelshHealthAndSafetyQuestion_08_Description', " + healthAndSafety08OptionsWithInsetTextJson + $@", NULL, 'WelshHealthAndSafetyQuestion_08_SummaryTitle', '_WelshHealthAndSafetyQuestion08', 14)
                    RETURNING id INTO q8Id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, sectionId, 3, true, 'WelshHealthAndSafetyQuestion_09_Title', NULL, " + healthAndSafety06GroupedOptionsJson + $@", NULL, 'WelshHealthAndSafetyQuestion_09_SummaryTitle', '_WelshHealthAndSafetyQuestion09', 15)
                    RETURNING id INTO q9Id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, sectionId, 2, false, 'WelshHealthAndSafetyQuestion_10_Title', 'WelshHealthAndSafetyQuestion_10_Description', " + healthAndSafety07GroupedOptionsJson + $@", NULL, 'WelshHealthAndSafetyQuestion_10_SummaryTitle', '_WelshHealthAndSafetyQuestion10', 16)
                    RETURNING id INTO q10Id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, sectionId, 2, false, 'WelshHealthAndSafetyQuestion_11_Title', NULL, " + healthAndSafety11OptionsWithInsetTextJson + $@", NULL, 'WelshHealthAndSafetyQuestion_11_SummaryTitle', '_WelshHealthAndSafetyQuestion11', 17)
                    RETURNING id INTO q11Id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, sectionId, 6, true, 'Global_CheckYourAnswers', '', '{{}}', NULL, NULL, '_WelshHealthAndSafetyQuestion12', 16)
                    RETURNING id INTO q12Id;

                    UPDATE form_questions SET next_question_id = q2Id WHERE id = q1Id;
                    UPDATE form_questions SET next_question_id = q3Id, next_question_alternative_id = q4Id WHERE id = q2Id;
                    UPDATE form_questions SET next_question_id = q4Id WHERE id = q3Id;
                    UPDATE form_questions SET next_question_id = q5Id, next_question_alternative_id = q6Id WHERE id = q4Id;
                    UPDATE form_questions SET next_question_id = q5Id_1_Id WHERE id = q5Id;
                    UPDATE form_questions SET next_question_id = q5Id_2_Id WHERE id = q5Id_1_Id;
                    UPDATE form_questions SET next_question_id = q5Id_3_Id WHERE id = q5Id_2_Id;
                    UPDATE form_questions SET next_question_id = q6Id WHERE id = q5Id_3_Id;
                    UPDATE form_questions SET next_question_id = q7Id, next_question_alternative_id = q8Id WHERE id = q6Id;
                    UPDATE form_questions SET next_question_id = q7Id_1_Id WHERE id = q7Id;
                    UPDATE form_questions SET next_question_id = q7Id_2_Id WHERE id = q7Id_1_Id;
                    UPDATE form_questions SET next_question_id = q7Id_3_Id WHERE id = q7Id_2_Id;
                    UPDATE form_questions SET next_question_id = q8Id WHERE id = q7Id_3_Id;
                    UPDATE form_questions SET next_question_id = q9Id WHERE id = q8Id;
                    UPDATE form_questions SET next_question_id = q10Id WHERE id = q9Id;
                    UPDATE form_questions SET next_question_id = q11Id WHERE id = q10Id;
                    UPDATE form_questions SET next_question_id = q12Id WHERE id = q11Id;
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
                    SELECT id INTO v_section_id FROM form_sections WHERE title = 'WelshHealthAndSafety_SectionTitle';

                    IF v_section_id IS NOT NULL THEN
                        DELETE FROM form_questions WHERE section_id = v_section_id;
                        DELETE FROM form_sections WHERE id = v_section_id;
                    END IF;
                END $$;
            ");
        }
    }
}
