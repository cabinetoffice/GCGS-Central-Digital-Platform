using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateWelshExclusions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var group1Id = new Guid("12345678-1234-1234-1234-123456789001");
            var group2Id = new Guid("12345678-1234-1234-1234-123456789002");
            var group3Id = new Guid("12345678-1234-1234-1234-123456789003");

            var group1SummaryTitle = "WelshExclusions_Group1_SummaryTitle";
            var group2SummaryTitle = "WelshExclusions_Group2_SummaryTitle";
            var group3SummaryTitle = "WelshExclusions_Group3_SummaryTitle";

            var group1GroupingJsonFragment =
                $"\"grouping\": {{ \"id\": \"{group1Id}\", \"page\": false, \"checkYourAnswers\": true, \"summaryTitle\": \"{group1SummaryTitle}\" }}";
            var group2GroupingJsonFragment =
                $"\"grouping\": {{ \"id\": \"{group2Id}\", \"page\": false, \"checkYourAnswers\": true, \"summaryTitle\": \"{group2SummaryTitle}\" }}";
            var group3GroupingJsonFragment =
                $"\"grouping\": {{ \"id\": \"{group3Id}\", \"page\": false, \"checkYourAnswers\": true, \"summaryTitle\": \"{group3SummaryTitle}\" }}";

            var group1GroupedOptionsJson = $"'{{ {group1GroupingJsonFragment} }}'";
            var group2GroupedOptionsJson = $"'{{ {group2GroupingJsonFragment} }}'";
            var group3GroupedOptionsJson = $"'{{ {group3GroupingJsonFragment} }}'";

            // _WelshExclusions03: Date (7) to MultiLine (10)
            migrationBuilder.Sql(@"
                UPDATE form_questions
                SET type = 10
                WHERE name = '_WelshExclusions03'
                AND section_id IN (
                    SELECT id
                    FROM form_sections
                    WHERE form_id = (SELECT id FROM forms WHERE name = 'Standard Questions')
                );
            ");

            // Add description to _WelshExclusions03
            migrationBuilder.Sql(@"
                UPDATE form_questions
                SET description = 'WelshExclusions_03_Description'
                WHERE name = '_WelshExclusions03'
                AND section_id IN (
                    SELECT id
                    FROM form_sections
                    WHERE form_id = (SELECT id FROM forms WHERE name = 'Standard Questions')
                );
            ");

            // _WelshExclusions04: YesOrNo (3) to MultiLine (10)
            migrationBuilder.Sql(@"
                UPDATE form_questions
                SET type = 10
                WHERE name = '_WelshExclusions04'
                AND section_id IN (
                    SELECT id
                    FROM form_sections
                    WHERE form_id = (SELECT id FROM forms WHERE name = 'Standard Questions')
                );
            ");

            // Change _WelshExclusions04 to required
            migrationBuilder.Sql(@"
                UPDATE form_questions
                SET is_required = true
                WHERE name = '_WelshExclusions04'
                AND section_id IN (
                    SELECT id
                    FROM form_sections
                    WHERE form_id = (SELECT id FROM forms WHERE name = 'Standard Questions')
                );
            ");

            // Redirect _WelshExclusions04 to point to _WelshExclusions06 (Check Your Answers)
            migrationBuilder.Sql(@"
                UPDATE form_questions
                SET next_question_id = (
                    SELECT id
                    FROM form_questions
                    WHERE name = '_WelshExclusions06'
                    AND section_id IN (
                        SELECT id
                        FROM form_sections
                        WHERE form_id = (SELECT id FROM forms WHERE name = 'Standard Questions')
                    )
                )
                WHERE name = '_WelshExclusions04'
                AND section_id IN (
                    SELECT id
                    FROM form_sections
                    WHERE form_id = (SELECT id FROM forms WHERE name = 'Standard Questions')
                );
            ");

            // Add grouping to _WelshExclusions02
            migrationBuilder.Sql($@"
                UPDATE form_questions
                SET options = {group1GroupedOptionsJson}
                WHERE name = '_WelshExclusions02'
                AND section_id IN (
                    SELECT id
                    FROM form_sections
                    WHERE form_id = (SELECT id FROM forms WHERE name = 'Standard Questions')
                );
            ");

            // Add grouping to _WelshExclusions03
            migrationBuilder.Sql($@"
                UPDATE form_questions
                SET options = {group2GroupedOptionsJson}
                WHERE name = '_WelshExclusions03'
                AND section_id IN (
                    SELECT id
                    FROM form_sections
                    WHERE form_id = (SELECT id FROM forms WHERE name = 'Standard Questions')
                );
            ");

            // Add grouping to _WelshExclusions04
            migrationBuilder.Sql($@"
                UPDATE form_questions
                SET options = {group3GroupedOptionsJson}
                WHERE name = '_WelshExclusions04'
                AND section_id IN (
                    SELECT id
                    FROM form_sections
                    WHERE form_id = (SELECT id FROM forms WHERE name = 'Standard Questions')
                );
            ");

            // 1) Break any links that point to _WelshExclusions05
            migrationBuilder.Sql(@"
                UPDATE form_questions
                SET next_question_id = NULL
                WHERE next_question_id IN (
                    SELECT id
                    FROM form_questions
                    WHERE name = '_WelshExclusions05'
                    AND section_id IN (
                        SELECT id
                        FROM form_sections
                        WHERE form_id = (SELECT id FROM forms WHERE name = 'Standard Questions')
                    )
                );
            ");

            // 2) Remove _WelshExclusions05
            migrationBuilder.Sql(@"
                DELETE FROM form_questions
                WHERE name = '_WelshExclusions05'
                AND section_id IN (
                    SELECT id
                    FROM form_sections
                    WHERE form_id = (SELECT id FROM forms WHERE name = 'Standard Questions')
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restore _WelshExclusions05 and restore the original links:
            // _WelshExclusions04 -> _WelshExclusions05 -> _WelshExclusions06
            migrationBuilder.Sql($@"
DO $$
DECLARE
    v_section_id INT;
    v_q05_id INT;
    v_q06_id INT;
BEGIN
    -- Find the Welsh Exclusions section by locating _WelshExclusions06 in Standard Questions
    SELECT fs.id INTO v_section_id
    FROM form_sections fs
    WHERE fs.form_id = (SELECT id FROM forms WHERE name = 'Standard Questions')
      AND EXISTS (
          SELECT 1
          FROM form_questions fq
          WHERE fq.section_id = fs.id
            AND fq.name = '_WelshExclusions06'
      )
    ORDER BY fs.id
    LIMIT 1;

    IF v_section_id IS NULL THEN
        RAISE EXCEPTION 'WelshExclusions section not found in Standard Questions (missing _WelshExclusions06)';
    END IF;

    -- Get CYA (_WelshExclusions06) id
    SELECT fq.id INTO v_q06_id
    FROM form_questions fq
    WHERE fq.name = '_WelshExclusions06'
      AND fq.section_id = v_section_id
    LIMIT 1;

    IF v_q06_id IS NULL THEN
        RAISE EXCEPTION '_WelshExclusions06 not found in Welsh Exclusions section';
    END IF;

    -- Recreate _WelshExclusions05 if missing (match OG: type=10, not required, summary title, sort_order=5)
    IF NOT EXISTS (
        SELECT 1 FROM form_questions
        WHERE name = '_WelshExclusions05'
          AND section_id = v_section_id
    ) THEN
        INSERT INTO form_questions
            (guid, next_question_id, next_question_alternative_id, section_id, ""type"",
             is_required, title, description, ""options"", caption, summary_title, ""name"", sort_order)
        VALUES
            ('{Guid.NewGuid()}', NULL, NULL, v_section_id, 10,
             false, 'WelshExclusions_05_Title', NULL, '{{}}'::jsonb, NULL, 'WelshExclusions_05_SummaryTitle', '_WelshExclusions05', 5)
        RETURNING id INTO v_q05_id;
    ELSE
        SELECT id INTO v_q05_id
        FROM form_questions
        WHERE name = '_WelshExclusions05'
          AND section_id = v_section_id
        LIMIT 1;
    END IF;

    -- Restore links
    UPDATE form_questions
    SET next_question_id = v_q05_id
    WHERE name = '_WelshExclusions04'
      AND section_id = v_section_id;

    UPDATE form_questions
    SET next_question_id = v_q06_id
    WHERE name = '_WelshExclusions05'
      AND section_id = v_section_id;

END $$;
");

            // Revert grouping for all three questions
            migrationBuilder.Sql(@"
                UPDATE form_questions
                SET options = '{}'
                WHERE name IN ('_WelshExclusions02', '_WelshExclusions03', '_WelshExclusions04')
                AND section_id IN (
                    SELECT id
                    FROM form_sections
                    WHERE form_id = (SELECT id FROM forms WHERE name = 'Standard Questions')
                );
            ");

            // Revert _WelshExclusions04 to not required
            migrationBuilder.Sql(@"
                UPDATE form_questions
                SET is_required = false
                WHERE name = '_WelshExclusions04'
                AND section_id IN (
                    SELECT id
                    FROM form_sections
                    WHERE form_id = (SELECT id FROM forms WHERE name = 'Standard Questions')
                );
            ");

            // Revert _WelshExclusions04 back to YesOrNo
            migrationBuilder.Sql(@"
                UPDATE form_questions
                SET type = 3
                WHERE name = '_WelshExclusions04'
                AND section_id IN (
                    SELECT id
                    FROM form_sections
                    WHERE form_id = (SELECT id FROM forms WHERE name = 'Standard Questions')
                );
            ");

            // Revert _WelshExclusions03 description
            migrationBuilder.Sql(@"
                UPDATE form_questions
                SET description = NULL
                WHERE name = '_WelshExclusions03'
                AND section_id IN (
                    SELECT id
                    FROM form_sections
                    WHERE form_id = (SELECT id FROM forms WHERE name = 'Standard Questions')
                );
            ");

            // Revert _WelshExclusions03 back to Date
            migrationBuilder.Sql(@"
                UPDATE form_questions
                SET type = 7
                WHERE name = '_WelshExclusions03'
                AND section_id IN (
                    SELECT id
                    FROM form_sections
                    WHERE form_id = (SELECT id FROM forms WHERE name = 'Standard Questions')
                );
            ");
        }
    }
}

