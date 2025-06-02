using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSteelSectionForm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                DO $$
                DECLARE
                    sectionId INT;
                    checkYourAnswersId INT;
                    steelQuestion03Id INT;
                BEGIN
                    SELECT id INTO sectionId FROM form_sections WHERE title = 'Steel_SectionTitle';

                    SELECT id INTO checkYourAnswersId FROM form_questions
                    WHERE section_id = sectionId AND name = '_SteelQuestion09';

                    SELECT id INTO steelQuestion03Id FROM form_questions
                    WHERE section_id = sectionId AND name = '_SteelQuestion03';

                    UPDATE form_questions
                    SET next_question_id = checkYourAnswersId
                    WHERE next_question_id IN (
                        SELECT id FROM form_questions
                        WHERE section_id = sectionId
                        AND name IN ('_SteelQuestion04', '_SteelQuestion05', '_SteelQuestion06', '_SteelQuestion07', '_SteelQuestion08')
                    );

                    UPDATE form_questions
                    SET next_question_alternative_id = NULL
                    WHERE next_question_alternative_id IN (
                        SELECT id FROM form_questions
                        WHERE section_id = sectionId
                        AND name IN ('_SteelQuestion04', '_SteelQuestion05', '_SteelQuestion06', '_SteelQuestion07', '_SteelQuestion08')
                    );

                    UPDATE form_questions
                    SET next_question_id = checkYourAnswersId
                    WHERE id = steelQuestion03Id;

                    DELETE FROM form_answers
                    WHERE question_id IN (
                        SELECT id FROM form_questions
                        WHERE section_id = sectionId
                        AND name IN ('_SteelQuestion04', '_SteelQuestion05', '_SteelQuestion06', '_SteelQuestion07', '_SteelQuestion08')
                    );

                    DELETE FROM form_questions
                    WHERE section_id = sectionId
                    AND name IN ('_SteelQuestion04', '_SteelQuestion05', '_SteelQuestion06', '_SteelQuestion07', '_SteelQuestion08');

                    UPDATE form_questions
                    SET name = '_SteelQuestion04', sort_order = 4
                    WHERE id = checkYourAnswersId;

                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                DO $$
                DECLARE
                    form_id int;
                    sectionId INT;
                    previousQuestionId INT;
                    steelQuestion03Id INT;
                    checkYourAnswersId INT;
                BEGIN
                    SELECT id INTO form_id FROM forms WHERE name = 'Standard Questions';

                    SELECT id INTO sectionId FROM form_sections WHERE title = 'Steel_SectionTitle';

                    SELECT id INTO steelQuestion03Id FROM form_questions
                    WHERE section_id = sectionId AND name = '_SteelQuestion03';

                    SELECT id INTO checkYourAnswersId FROM form_questions
                    WHERE section_id = sectionId AND name = '_SteelQuestion04';

                    UPDATE form_questions
                    SET name = '_SteelQuestion09', sort_order = 9
                    WHERE id = checkYourAnswersId;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', checkYourAnswersId, NULL, sectionId, 2, false, 'SteelQuestion_08_Title', '', '{{}}', NULL, 'SteelQuestion_08_SummaryTitle', '_SteelQuestion08', 8)
                    RETURNING id INTO previousQuestionId;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', previousQuestionId, NULL, sectionId, 10, true, 'SteelQuestion_07_Title', 'SteelQuestion_07_Description', '{{}}', NULL, 'SteelQuestion_07_SummaryTitle', '_SteelQuestion07', 7)
                    RETURNING id INTO previousQuestionId;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', previousQuestionId, NULL, sectionId, 3, false, 'SteelQuestion_06_Title', '', '{{}}', NULL, 'SteelQuestion_06_SummaryTitle', '_SteelQuestion06', 6)
                    RETURNING id INTO previousQuestionId;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', previousQuestionId, NULL, sectionId, 2, false, 'SteelQuestion_05_Title', '', '{{}}', NULL, 'SteelQuestion_05_SummaryTitle', '_SteelQuestion05', 5)
                    RETURNING id INTO previousQuestionId;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', previousQuestionId, NULL, sectionId, 10, true, 'SteelQuestion_04_Title', 'SteelQuestion_04_Description', '{{}}', NULL, 'SteelQuestion_04_SummaryTitle', '_SteelQuestion04', 4)
                    RETURNING id INTO previousQuestionId;

                    UPDATE form_questions
                    SET next_question_id = previousQuestionId
                    WHERE id = steelQuestion03Id;

                END $$;
            ");
        }
    }
}
