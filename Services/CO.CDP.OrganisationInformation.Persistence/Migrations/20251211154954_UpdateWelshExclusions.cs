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

            var group1GroupingJsonFragment = $"\"grouping\": {{ \"id\": \"{group1Id}\", \"page\": false, \"checkYourAnswers\": true, \"summaryTitle\": \"{group1SummaryTitle}\" }}";
            var group2GroupingJsonFragment = $"\"grouping\": {{ \"id\": \"{group2Id}\", \"page\": false, \"checkYourAnswers\": true, \"summaryTitle\": \"{group2SummaryTitle}\" }}";
            var group3GroupingJsonFragment = $"\"grouping\": {{ \"id\": \"{group3Id}\", \"page\": false, \"checkYourAnswers\": true, \"summaryTitle\": \"{group3SummaryTitle}\" }}";

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

            // Revert _WelshExclusions04 next_question_id
            migrationBuilder.Sql(@"
                UPDATE form_questions
                SET next_question_id = NULL
                WHERE name = '_WelshExclusions04'
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
