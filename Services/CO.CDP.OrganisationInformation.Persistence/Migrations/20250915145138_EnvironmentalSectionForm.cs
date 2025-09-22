using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnvironmentalSectionForm : Migration
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
                    VALUES ('{Guid.NewGuid()}', 'Environmental_SectionTitle', form_id, FALSE, FALSE, 4, 2, '{{}}');

                    SELECT id INTO sectionId FROM form_sections where title = 'Environmental_SectionTitle';

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, sectionId, 6, true, 'Global_CheckYourAnswers', '', '{{}}', NULL, NULL, '_EnvironmentalQuestion06', 6)
                    RETURNING id INTO previousQuestionId;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', previousQuestionId, NULL, sectionId, 10, false, 'EnvironmentalQuestion_05_Title', NULL, '{{}}', NULL, 'EnvironmentalQuestion_05_SummaryTitle', '_EnvironmentalQuestion05', 5)
                    RETURNING id INTO previousQuestionId;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', previousQuestionId, NULL, sectionId, 2, false, 'EnvironmentalQuestion_04_Title', NULL, '{{""layout"": {{""button"" : {{""beforeButtonContent"": ""EnvironmentalQuestion_04_CustomInsetText""}}}}}}', NULL, 'EnvironmentalQuestion_04_SummaryTitle', '_EnvironmentalQuestion04', 4)
                    RETURNING id INTO previousQuestionId;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', previousQuestionId, NULL, sectionId, 10, false, 'EnvironmentalQuestion_03_Title', 'EnvironmentalQuestion_03_Description', '{{}}', NULL, 'EnvironmentalQuestion_03_SummaryTitle', '_EnvironmentalQuestion03', 3)
                    RETURNING id INTO previousQuestionId;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', previousQuestionId, NULL, sectionId, 2, false, 'EnvironmentalQuestion_02_Title', NULL, '{{""layout"": {{""button"" : {{""beforeButtonContent"": ""EnvironmentalQuestion_02_CustomInsetText""}}}}}}', NULL, 'EnvironmentalQuestion_02_SummaryTitle', '_EnvironmentalQuestion02', 2)
                    RETURNING id INTO previousQuestionId;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', previousQuestionId, NULL, sectionId, 0, true, 'EnvironmentalQuestion_01_Title', 'EnvironmentalQuestion_01_Description', '{{""layout"": {{""button"": {{""style"": ""Start"", ""text"": ""Global_Start""}}}}}}', NULL, '', '_EnvironmentalQuestion01', 1)
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
                    SELECT id INTO sectionId FROM form_sections WHERE title = 'Environmental_SectionTitle';
                    DELETE FROM form_questions WHERE section_id = sectionId;
                    DELETE FROM form_sections WHERE id = sectionId;
                END $$;
            ");
        }
    }
}
