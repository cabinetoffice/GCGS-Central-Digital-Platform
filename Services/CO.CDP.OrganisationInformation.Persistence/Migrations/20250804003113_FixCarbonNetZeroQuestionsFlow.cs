using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixCarbonNetZeroQuestionsFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var carbonReductionPlanGroupId = Guid.NewGuid();
            var baselineEmissionsGroupId = Guid.NewGuid();
            var reportingEmissionsGroupId = Guid.NewGuid();

            var carbonReductionPlanSummaryTitle = "CarbonNetZero_CarbonReductionPlan";
            var baselineEmissionsSummaryTitle = "CarbonNetZero_BaselineEmissions_Group";
            var reportingEmissionsSummaryTitle = "CarbonNetZero_ReportingEmissions_Group";

            var carbonReductionPlanGroupingJsonFragment = $"\"grouping\": {{ \"id\": \"{carbonReductionPlanGroupId}\", \"page\": false, \"checkYourAnswers\": true, \"summaryTitle\": \"{carbonReductionPlanSummaryTitle}\" }}";
            var baselineEmissionsGroupingJsonFragment = $"\"grouping\": {{ \"id\": \"{baselineEmissionsGroupId}\", \"page\": true, \"checkYourAnswers\": true, \"summaryTitle\": \"{baselineEmissionsSummaryTitle}\" }}";
            var reportingEmissionsGroupingJsonFragment = $"\"grouping\": {{ \"id\": \"{reportingEmissionsGroupId}\", \"page\": true, \"checkYourAnswers\": true, \"summaryTitle\": \"{reportingEmissionsSummaryTitle}\" }}";

            var carbonReductionPlanOptionsJson = $"'{{ {carbonReductionPlanGroupingJsonFragment} }}'";
            var carbonReductionPlanYearOptionsJson = $"'{{ \"layout\": {{ \"inputWidth\": 3 }}, \"validation\": {{ \"textValidationType\": \"Year\" }}, {carbonReductionPlanGroupingJsonFragment} }}'";
            var carbonReductionPlanExpiryOptionsJson = $"'{{ \"validation\": {{ \"dateValidationType\": \"FutureOnly\" }}, {carbonReductionPlanGroupingJsonFragment} }}'";

            var baselineYearOptionsJson = $"'{{ \"layout\": {{ \"inputWidth\": 3 }}, \"validation\": {{ \"textValidationType\": \"Year\" }}, {baselineEmissionsGroupingJsonFragment} }}'";
            var baselineEmissionOptionsJson = $"'{{ \"layout\": {{ \"inputWidth\": 3, \"inputSuffix\": {{ \"type\": 0, \"text\": \"CarbonNetZero_tCO2e\" }} }}, \"validation\": {{ \"textValidationType\": \"Decimal\" }}, {baselineEmissionsGroupingJsonFragment} }}'";

            var reportingYearOptionsJson = $"'{{ \"layout\": {{ \"inputWidth\": 3 }}, \"validation\": {{ \"textValidationType\": \"Year\" }}, {reportingEmissionsGroupingJsonFragment} }}'";
            var reportingEmissionOptionsJson = $"'{{ \"layout\": {{ \"inputWidth\": 3, \"inputSuffix\": {{ \"type\": 0, \"text\": \"CarbonNetZero_tCO2e\" }} }}, \"validation\": {{ \"textValidationType\": \"Decimal\" }}, {reportingEmissionsGroupingJsonFragment} }}'";

            var baselineNoInputOptionsJson = $"'{{ {baselineEmissionsGroupingJsonFragment} }}'";
            var reportingNoInputOptionsJson = $"'{{ {reportingEmissionsGroupingJsonFragment} }}'";

            var baselineContainerGuid = Guid.NewGuid();
            var baselineLabelGuid = Guid.NewGuid();
            var reportingContainerGuid = Guid.NewGuid();
            var reportingLabelGuid = Guid.NewGuid();

            var sql = $@"
                DO $$
                DECLARE
                    section_id_val INT;
                    baseline_container_id INT;
                    reporting_container_id INT;
                    cya_id INT;
                BEGIN
                    SELECT id INTO section_id_val FROM form_sections WHERE title = 'CarbonNetZero_SectionTitle';
                    SELECT id INTO cya_id FROM form_questions WHERE title = 'Global_CheckYourAnswers' AND section_id = section_id_val;

                    -- Clear all navigation within the carbon net zero section
                    UPDATE form_questions
                    SET next_question_id = NULL, next_question_alternative_id = NULL
                    WHERE section_id = section_id_val;

                    -- Delete any existing container/label questions that might conflict
                    DELETE FROM form_questions WHERE name IN (
                        '_CarbonNetZeroBaselineContainer', '_CarbonNetZeroBaselineLabel',
                        '_CarbonNetZeroReportingContainer', '_CarbonNetZeroReportingLabel'
                    );

                    -- Reset all existing questions to clean state
                    UPDATE form_questions
                    SET options = '{{}}', summary_title = NULL
                    WHERE title IN (
                        'CarbonNetZero_02_Title', 'CarbonNetZero_03_Title', 'CarbonNetZero_04_Title', 'CarbonNetZero_05_Title',
                        'CarbonNetZero_06_Title', 'CarbonNetZero_07_Title', 'CarbonNetZero_08_Title', 'CarbonNetZero_09_Title',
                        'CarbonNetZero_10_Title', 'CarbonNetZero_11_Title', 'CarbonNetZero_12_Title', 'CarbonNetZero_13_Title'
                    );

                    -- Update question grouping and formatting
                    -- Group 1: Carbon Reduction Plan questions (02-05)
                    UPDATE form_questions SET options = {carbonReductionPlanOptionsJson} WHERE title = 'CarbonNetZero_02_Title';
                    UPDATE form_questions SET options = {carbonReductionPlanOptionsJson} WHERE title = 'CarbonNetZero_03_Title';
                    UPDATE form_questions SET options = {carbonReductionPlanExpiryOptionsJson}, caption = 'DateForm_FutureDate_Hint' WHERE title = 'CarbonNetZero_04_Title';
                    UPDATE form_questions SET options = {carbonReductionPlanYearOptionsJson} WHERE title = 'CarbonNetZero_05_Title';

                    -- Insert baseline emissions container and label
                    INSERT INTO form_questions (guid, section_id, ""type"", is_required, title, description, ""options"", caption, ""name"", sort_order)
                    VALUES ('{baselineContainerGuid}', section_id_val, 0, false, 'CarbonNetZero_06_BaselineContainer_Title', NULL, {baselineNoInputOptionsJson}, 'CarbonNetZero_CarbonNetZeroDetails_SupplierEmissionsDeclaration_Text', '_CarbonNetZeroBaselineContainer', 5.1);

                    INSERT INTO form_questions (guid, section_id, ""type"", is_required, title, description, ""options"", caption, ""name"", sort_order)
                    VALUES ('{baselineLabelGuid}', section_id_val, 0, false, 'CarbonNetZero_CarbonNetZeroDetails_RelevantEmissionsData_Heading', NULL, {baselineNoInputOptionsJson}, NULL, '_CarbonNetZeroBaselineLabel', 5.2);

                    -- Update baseline questions (06-09) with correct grouping and formatting
                    UPDATE form_questions SET options = {baselineYearOptionsJson}, summary_title = 'CarbonNetZero_06_SummaryTitle' WHERE title = 'CarbonNetZero_06_Title';
                    UPDATE form_questions SET options = {baselineEmissionOptionsJson}, summary_title = 'CarbonNetZero_07_SummaryTitle' WHERE title = 'CarbonNetZero_07_Title';
                    UPDATE form_questions SET options = {baselineEmissionOptionsJson}, summary_title = 'CarbonNetZero_08_SummaryTitle' WHERE title = 'CarbonNetZero_08_Title';
                    UPDATE form_questions SET options = {baselineEmissionOptionsJson}, summary_title = 'CarbonNetZero_09_SummaryTitle' WHERE title = 'CarbonNetZero_09_Title';

                    -- Insert reporting emissions container and label
                    INSERT INTO form_questions (guid, section_id, ""type"", is_required, title, description, ""options"", caption, ""name"", sort_order)
                    VALUES ('{reportingContainerGuid}', section_id_val, 0, false, 'CarbonNetZero_10_ReportingContainer_Title', NULL, {reportingNoInputOptionsJson}, 'CarbonNetZero_CarbonNetZeroDetails_SupplierEmissionsDeclaration_Text', '_CarbonNetZeroReportingContainer', 9.1);

                    INSERT INTO form_questions (guid, section_id, ""type"", is_required, title, description, ""options"", caption, ""name"", sort_order)
                    VALUES ('{reportingLabelGuid}', section_id_val, 0, false, 'CarbonNetZero_CarbonNetZeroDetails_RelevantEmissionsData_Heading', NULL, {reportingNoInputOptionsJson}, NULL, '_CarbonNetZeroReportingLabel', 9.2);

                    -- Update reporting questions (10-13) with correct grouping and formatting
                    UPDATE form_questions SET options = {reportingYearOptionsJson}, summary_title = 'CarbonNetZero_10_SummaryTitle' WHERE title = 'CarbonNetZero_10_Title';
                    UPDATE form_questions SET options = {reportingEmissionOptionsJson}, summary_title = 'CarbonNetZero_11_SummaryTitle' WHERE title = 'CarbonNetZero_11_Title';
                    UPDATE form_questions SET options = {reportingEmissionOptionsJson}, summary_title = 'CarbonNetZero_12_SummaryTitle' WHERE title = 'CarbonNetZero_12_Title';
                    UPDATE form_questions SET options = {reportingEmissionOptionsJson}, summary_title = 'CarbonNetZero_13_SummaryTitle' WHERE title = 'CarbonNetZero_13_Title';

                    -- Get the IDs of the container questions we just created
                    SELECT id INTO baseline_container_id FROM form_questions WHERE name = '_CarbonNetZeroBaselineContainer';
                    SELECT id INTO reporting_container_id FROM form_questions WHERE name = '_CarbonNetZeroReportingContainer';

                    -- Rebuild navigation chain: 01 -> 02 -> 03 -> 04 -> 05 -> Baseline Container -> Baseline Label -> 06 -> 07 -> 08 -> 09 -> Reporting Container -> Reporting Label -> 10 -> 11 -> 12 -> 13 -> CYA
                    UPDATE form_questions SET next_question_id = (SELECT id FROM form_questions WHERE title = 'CarbonNetZero_02_Title') WHERE title = 'CarbonNetZero_01_Title';
                    UPDATE form_questions SET next_question_id = (SELECT id FROM form_questions WHERE title = 'CarbonNetZero_03_Title') WHERE title = 'CarbonNetZero_02_Title';
                    UPDATE form_questions SET next_question_id = (SELECT id FROM form_questions WHERE title = 'CarbonNetZero_04_Title') WHERE title = 'CarbonNetZero_03_Title';
                    UPDATE form_questions SET next_question_id = (SELECT id FROM form_questions WHERE title = 'CarbonNetZero_05_Title') WHERE title = 'CarbonNetZero_04_Title';
                    UPDATE form_questions SET next_question_id = baseline_container_id WHERE title = 'CarbonNetZero_05_Title';

                    -- Baseline container -> baseline label -> first baseline question
                    UPDATE form_questions SET next_question_id = (SELECT id FROM form_questions WHERE name = '_CarbonNetZeroBaselineLabel') WHERE name = '_CarbonNetZeroBaselineContainer';
                    UPDATE form_questions SET next_question_id = (SELECT id FROM form_questions WHERE title = 'CarbonNetZero_06_Title') WHERE name = '_CarbonNetZeroBaselineLabel';

                    -- Baseline questions flow: 06 -> 07 -> 08 -> 09
                    UPDATE form_questions SET next_question_id = (SELECT id FROM form_questions WHERE title = 'CarbonNetZero_07_Title') WHERE title = 'CarbonNetZero_06_Title';
                    UPDATE form_questions SET next_question_id = (SELECT id FROM form_questions WHERE title = 'CarbonNetZero_08_Title') WHERE title = 'CarbonNetZero_07_Title';
                    UPDATE form_questions SET next_question_id = (SELECT id FROM form_questions WHERE title = 'CarbonNetZero_09_Title') WHERE title = 'CarbonNetZero_08_Title';
                    UPDATE form_questions SET next_question_id = reporting_container_id WHERE title = 'CarbonNetZero_09_Title';

                    -- Reporting container -> reporting label -> first reporting question
                    UPDATE form_questions SET next_question_id = (SELECT id FROM form_questions WHERE name = '_CarbonNetZeroReportingLabel') WHERE name = '_CarbonNetZeroReportingContainer';
                    UPDATE form_questions SET next_question_id = (SELECT id FROM form_questions WHERE title = 'CarbonNetZero_10_Title') WHERE name = '_CarbonNetZeroReportingLabel';

                    -- Reporting questions flow: 10 -> 11 -> 12 -> 13 -> CYA
                    UPDATE form_questions SET next_question_id = (SELECT id FROM form_questions WHERE title = 'CarbonNetZero_11_Title') WHERE title = 'CarbonNetZero_10_Title';
                    UPDATE form_questions SET next_question_id = (SELECT id FROM form_questions WHERE title = 'CarbonNetZero_12_Title') WHERE title = 'CarbonNetZero_11_Title';
                    UPDATE form_questions SET next_question_id = (SELECT id FROM form_questions WHERE title = 'CarbonNetZero_13_Title') WHERE title = 'CarbonNetZero_12_Title';
                    UPDATE form_questions SET next_question_id = cya_id WHERE title = 'CarbonNetZero_13_Title';

                END $$;
            ";

            migrationBuilder.Sql(sql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var sql = @"
                DO $$
                DECLARE
                    section_id_val INT;
                    cya_id INT;
                BEGIN
                    SELECT id INTO section_id_val FROM form_sections WHERE title = 'CarbonNetZero_SectionTitle';
                    SELECT id INTO cya_id FROM form_questions WHERE title = 'Global_CheckYourAnswers' AND section_id = section_id_val;

                    -- Clear all navigation within carbon net zero section
                    UPDATE form_questions
                    SET next_question_id = NULL, next_question_alternative_id = NULL
                    WHERE section_id = section_id_val;

                    -- Remove the container questions we added
                    DELETE FROM form_questions WHERE name IN (
                        '_CarbonNetZeroBaselineContainer', '_CarbonNetZeroBaselineLabel',
                        '_CarbonNetZeroReportingContainer', '_CarbonNetZeroReportingLabel'
                    );

                    -- Reset all questions to clean state
                    UPDATE form_questions
                    SET options = '{}', summary_title = NULL
                    WHERE title IN (
                        'CarbonNetZero_02_Title', 'CarbonNetZero_03_Title', 'CarbonNetZero_04_Title', 'CarbonNetZero_05_Title',
                        'CarbonNetZero_06_Title', 'CarbonNetZero_07_Title', 'CarbonNetZero_08_Title', 'CarbonNetZero_09_Title',
                        'CarbonNetZero_10_Title', 'CarbonNetZero_11_Title', 'CarbonNetZero_12_Title', 'CarbonNetZero_13_Title'
                    );

                    -- Rebuild simple navigation flow
                    UPDATE form_questions SET next_question_id = (SELECT id FROM form_questions f WHERE f.title = 'CarbonNetZero_02_Title') WHERE title = 'CarbonNetZero_01_Title';
                    UPDATE form_questions SET next_question_id = (SELECT id FROM form_questions f WHERE f.title = 'CarbonNetZero_03_Title') WHERE title = 'CarbonNetZero_02_Title';
                    UPDATE form_questions SET next_question_id = (SELECT id FROM form_questions f WHERE f.title = 'CarbonNetZero_04_Title') WHERE title = 'CarbonNetZero_03_Title';
                    UPDATE form_questions SET next_question_id = (SELECT id FROM form_questions f WHERE f.title = 'CarbonNetZero_05_Title') WHERE title = 'CarbonNetZero_04_Title';
                    UPDATE form_questions SET next_question_id = (SELECT id FROM form_questions f WHERE f.title = 'CarbonNetZero_06_Title') WHERE title = 'CarbonNetZero_05_Title';
                    UPDATE form_questions SET next_question_id = (SELECT id FROM form_questions f WHERE f.title = 'CarbonNetZero_07_Title') WHERE title = 'CarbonNetZero_06_Title';
                    UPDATE form_questions SET next_question_id = (SELECT id FROM form_questions f WHERE f.title = 'CarbonNetZero_08_Title') WHERE title = 'CarbonNetZero_07_Title';
                    UPDATE form_questions SET next_question_id = (SELECT id FROM form_questions f WHERE f.title = 'CarbonNetZero_09_Title') WHERE title = 'CarbonNetZero_08_Title';
                    UPDATE form_questions SET next_question_id = (SELECT id FROM form_questions f WHERE f.title = 'CarbonNetZero_10_Title') WHERE title = 'CarbonNetZero_09_Title';
                    UPDATE form_questions SET next_question_id = (SELECT id FROM form_questions f WHERE f.title = 'CarbonNetZero_11_Title') WHERE title = 'CarbonNetZero_10_Title';
                    UPDATE form_questions SET next_question_id = (SELECT id FROM form_questions f WHERE f.title = 'CarbonNetZero_12_Title') WHERE title = 'CarbonNetZero_11_Title';
                    UPDATE form_questions SET next_question_id = (SELECT id FROM form_questions f WHERE f.title = 'CarbonNetZero_13_Title') WHERE title = 'CarbonNetZero_12_Title';
                    UPDATE form_questions SET next_question_id = cya_id WHERE title = 'CarbonNetZero_13_Title';

                END $$;
            ";
            migrationBuilder.Sql(sql);
        }
    }
}