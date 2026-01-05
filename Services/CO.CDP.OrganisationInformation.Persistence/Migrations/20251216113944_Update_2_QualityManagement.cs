
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Update_2_QualityManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var newQuestionGuid = Guid.NewGuid();

            migrationBuilder.Sql($@"
                DO $$
                DECLARE
                    sectionId INT;
                    question02Id INT;
                    question03Id INT;
                    question04Id INT;
                    question05Id INT;
                BEGIN
                    -- Find section
                    SELECT id INTO sectionId
                    FROM form_sections
                    WHERE title = 'QualityManagement_SectionTitle';

                    IF sectionId IS NULL THEN
                        RAISE EXCEPTION 'Section with title ''QualityManagement_SectionTitle'' not found';
                    END IF;

                    -- Get existing questions
                    SELECT id INTO question02Id
                    FROM form_questions
                    WHERE section_id = sectionId
                      AND name = '_QualityManagementQuestion02';

                    IF question02Id IS NULL THEN
                        RAISE EXCEPTION 'Question ''_QualityManagementQuestion02'' not found in section %', sectionId;
                    END IF;

                    SELECT id INTO question03Id
                    FROM form_questions
                    WHERE section_id = sectionId
                      AND name = '_QualityManagementQuestion03';

                    IF question03Id IS NULL THEN
                        RAISE EXCEPTION 'Question ''_QualityManagementQuestion03'' not found in section %', sectionId;
                    END IF;

                    SELECT id INTO question05Id
                    FROM form_questions
                    WHERE section_id = sectionId
                      AND name = '_QualityManagementQuestion04'
                      AND type = 6;

                    IF question05Id IS NULL THEN
                        RAISE EXCEPTION 'Question ''_QualityManagementQuestion04'' (type 6) not found in section %', sectionId;
                    END IF;

                    -- Add description to Question 02, remove custom inset text, and fix type
                    UPDATE form_questions
                    SET description = 'QualityManagementQuestion_02_Description',
                        ""options"" = '{{}}'::jsonb,
                        ""type"" = 3
                    WHERE id = question02Id;

                    -- Rename Check Your Answers from Question 04 to Question 05
                    UPDATE form_questions
                    SET name = '_QualityManagementQuestion05',
                        sort_order = 5
                    WHERE id = question05Id;

                    -- Rename old Question 03 to Question 04
                    UPDATE form_questions
                    SET name = '_QualityManagementQuestion04',
                        sort_order = 4,
                        title = 'QualityManagementQuestion_04_Title',
                        summary_title = 'QualityManagementQuestion_04_SummaryTitle'
                    WHERE id = question03Id;

                    -- Insert new Question 03 (file upload)
                    INSERT INTO form_questions
                        (guid, next_question_id, next_question_alternative_id, section_id, ""type"",
                         is_required, title, description, ""options"", caption, summary_title, ""name"", sort_order)
                    VALUES
                        ('{newQuestionGuid}', question03Id, NULL, sectionId, 2,
                         false, 'QualityManagementQuestion_03_Title', NULL,
                         '{{""layout"": {{""button"": {{""beforeButtonContent"": ""QualityManagementQuestion_03_CustomInsetText""}}}}}}'::jsonb, NULL,
                         'QualityManagementQuestion_03_SummaryTitle', '_QualityManagementQuestion03', 3)
                    RETURNING id INTO question04Id;

                    -- Set up branching: YES → Q3 (file upload), NO → Q4
                    UPDATE form_questions
                    SET next_question_id = question04Id,
                        next_question_alternative_id = question03Id
                    WHERE id = question02Id;

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
                    question02Id INT;
                    question03Id INT;
                    question04Id INT;
                    question05Id INT;
                BEGIN
                    SELECT id INTO sectionId
                    FROM form_sections
                    WHERE title = 'QualityManagement_SectionTitle';

                    IF sectionId IS NULL THEN
                        RAISE EXCEPTION 'Section with title ''QualityManagement_SectionTitle'' not found';
                    END IF;

                    SELECT id INTO question02Id
                    FROM form_questions
                    WHERE section_id = sectionId
                      AND name = '_QualityManagementQuestion02';

                    IF question02Id IS NULL THEN
                        RAISE EXCEPTION 'Question ''_QualityManagementQuestion02'' not found in section %', sectionId;
                    END IF;

                    SELECT id INTO question03Id
                    FROM form_questions
                    WHERE section_id = sectionId
                      AND name = '_QualityManagementQuestion03'
                      AND sort_order = 3;

                    IF question03Id IS NULL THEN
                        RAISE EXCEPTION 'Question ''_QualityManagementQuestion03'' (sort_order 3) not found in section %', sectionId;
                    END IF;

                    SELECT id INTO question04Id
                    FROM form_questions
                    WHERE section_id = sectionId
                      AND name = '_QualityManagementQuestion04'
                      AND sort_order = 4;

                    IF question04Id IS NULL THEN
                        RAISE EXCEPTION 'Question ''_QualityManagementQuestion04'' (sort_order 4) not found in section %', sectionId;
                    END IF;

                    SELECT id INTO question05Id
                    FROM form_questions
                    WHERE section_id = sectionId
                      AND name = '_QualityManagementQuestion05'
                      AND sort_order = 5;

                    IF question05Id IS NULL THEN
                        RAISE EXCEPTION 'Question ''_QualityManagementQuestion05'' (sort_order 5) not found in section %', sectionId;
                    END IF;

                    -- Clear Question 03's own foreign key references first
                    UPDATE form_questions
                    SET next_question_id = NULL,
                        next_question_alternative_id = NULL
                    WHERE id = question03Id;

                    -- Remove any other foreign key references to Question 03
                    UPDATE form_questions
                    SET next_question_id = CASE
                        WHEN next_question_id = question03Id THEN NULL
                        ELSE next_question_id
                    END,
                    next_question_alternative_id = CASE
                        WHEN next_question_alternative_id = question03Id THEN NULL
                        ELSE next_question_alternative_id
                    END
                    WHERE next_question_id = question03Id
                       OR next_question_alternative_id = question03Id;

                    -- Delete Question 03 (file upload)
                    DELETE FROM form_questions
                    WHERE id = question03Id;

                    -- Rename Question 04 back to Question 03
                    UPDATE form_questions
                    SET name = '_QualityManagementQuestion03',
                        sort_order = 3,
                        title = 'QualityManagementQuestion_03_Title',
                        summary_title = 'QualityManagementQuestion_03_SummaryTitle'
                    WHERE id = question04Id;

                    -- Rename Check Your Answers back to Question 04
                    UPDATE form_questions
                    SET name = '_QualityManagementQuestion04',
                        sort_order = 4
                    WHERE id = question05Id;

                    -- Restore original routing for Question 02
                    UPDATE form_questions
                    SET description = NULL,
                        caption = NULL,
                        ""options"" = '{{""layout"": {{""button"": {{""beforeButtonContent"": ""QualityManagementQuestion_02_CustomInsetText""}}}}}}'::jsonb,
                        ""type"" = 2,
                        next_question_id = question04Id,
                        next_question_alternative_id = NULL
                    WHERE id = question02Id;

                END $$;
            ");
        }
    }
}