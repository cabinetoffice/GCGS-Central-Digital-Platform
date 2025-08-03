using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class GroupPaymentsQuestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Use fixed GUIDs for grouping questions
            var payments01GroupId = new Guid("12345678-1234-1234-1234-123456789001"); // Group 1: Payments_02-05
            var payments06GroupId = new Guid("12345678-1234-1234-1234-123456789006"); // Group 2: Payments_06
            var payments07GroupId = new Guid("12345678-1234-1234-1234-123456789007"); // Group 3: Payments_07
            var payments04GroupId = new Guid("12345678-1234-1234-1234-123456789004"); // Group 4: Payments_08-11

            var payments01SummaryTitle = "Payments_Group1_SummaryTitle";
            var payments06SummaryTitle = "Payments_Group2_SummaryTitle";
            var payments07SummaryTitle = "Payments_Group3_SummaryTitle";
            var payments04SummaryTitle = "Payments_Group4_SummaryTitle";

            // Base JSON for grouping structures
            var payments01GroupingJsonFragment = $"\"grouping\": {{ \"id\": \"{payments01GroupId}\", \"page\": false, \"checkYourAnswers\": true, \"summaryTitle\": \"{payments01SummaryTitle}\" }}";
            var payments06GroupingJsonFragment = $"\"grouping\": {{ \"id\": \"{payments06GroupId}\", \"page\": true, \"checkYourAnswers\": true, \"summaryTitle\": \"{payments06SummaryTitle}\" }}";
            var payments07GroupingJsonFragment = $"\"grouping\": {{ \"id\": \"{payments07GroupId}\", \"page\": true, \"checkYourAnswers\": true, \"summaryTitle\": \"{payments07SummaryTitle}\" }}";
            var payments04GroupingJsonFragment = $"\"grouping\": {{ \"id\": \"{payments04GroupId}\", \"page\": false, \"checkYourAnswers\": true, \"summaryTitle\": \"{payments04SummaryTitle}\" }}";

            // JSON for percentage questions with inputWidth: 2 and inputSuffix as GovUkDefault with '%' resource key and percentage validation
            var payments06PercentageOptionsJson = $"'{{ \"layout\": {{ \"inputWidth\": 2, \"inputSuffix\": {{ \"type\": 0, \"text\": \"%\" }} }}, \"validation\": {{ \"textValidationType\": \"Percentage\" }}, {payments06GroupingJsonFragment} }}'";
            var payments07PercentageOptionsJson = $"'{{ \"layout\": {{ \"inputWidth\": 2, \"inputSuffix\": {{ \"type\": 0, \"text\": \"%\" }} }}, \"validation\": {{ \"textValidationType\": \"Percentage\" }}, {payments07GroupingJsonFragment} }}'";

            // JSON for days questions with inputWidth: 2 and inputSuffix as CustomText with 'Payments_Days' and number validation
            var payments06DaysOptionsJson = $"'{{ \"layout\": {{ \"inputWidth\": 2, \"inputSuffix\": {{ \"type\": 1, \"text\": \"Payments_Days\" }} }}, \"validation\": {{ \"textValidationType\": \"Number\" }}, {payments06GroupingJsonFragment} }}'";
            var payments07DaysOptionsJson = $"'{{ \"layout\": {{ \"inputWidth\": 2, \"inputSuffix\": {{ \"type\": 1, \"text\": \"Payments_Days\" }} }}, \"validation\": {{ \"textValidationType\": \"Number\" }}, {payments07GroupingJsonFragment} }}'";

            // JSON for regular grouped questions
            var payments01GroupedOptionsJson = $"'{{ {payments01GroupingJsonFragment} }}'";
            var payments06GroupedOptionsJson = $"'{{ {payments06GroupingJsonFragment} }}'";
            var payments07GroupedOptionsJson = $"'{{ {payments07GroupingJsonFragment} }}'";
            var payments04GroupedOptionsJson = $"'{{ {payments04GroupingJsonFragment} }}'";

            // JSON for Payments_11 with after button content
            var payments11OptionsJson = $"'{{ \"layout\": {{ \"afterButtonContent\": \"Payments_11_InsetText\" }}, {payments04GroupingJsonFragment} }}'";

            var payments06ReportingStartDateOptionsJson = $"'{{ \"validation\": {{ \"dateValidationType\": \"PastOnly\" }}, {payments06GroupingJsonFragment} }}'";
            var payments07ReportingStartDateOptionsJson = $"'{{ \"validation\": {{ \"dateValidationType\": \"PastOnly\" }}, {payments07GroupingJsonFragment} }}'";

            var sql = @"
                DO $$
                BEGIN
                    -- Update Group 1 questions (Payments_02-05) with grouping
                    UPDATE form_questions
                    SET options = " + payments01GroupedOptionsJson + @"
                    WHERE title = 'Payments_02_Title';

                    UPDATE form_questions
                    SET options = " + payments01GroupedOptionsJson + @"
                    WHERE title = 'Payments_03_Title';

                    UPDATE form_questions
                    SET options = " + payments01GroupedOptionsJson + @"
                    WHERE title = 'Payments_04_Title';

                    UPDATE form_questions
                    SET options = " + payments01GroupedOptionsJson + @"
                    WHERE title = 'Payments_05_Title';

                    -- Update Group 2 questions (Payments_06) with grouping
                    UPDATE form_questions
                    SET options = " + payments06GroupedOptionsJson + @"
                    WHERE title = 'Payments_06_Title';

                    UPDATE form_questions
                    SET options = " + payments06GroupedOptionsJson + @"
                    WHERE title = 'Payments_06_ReportingStartDate';

                    UPDATE form_questions
                    SET options = " + payments06DaysOptionsJson + @"
                    WHERE title = 'Payments_06_AverageDaysToPayInvoice';

                    UPDATE form_questions
                    SET options = " + payments06GroupedOptionsJson + @"
                    WHERE title = 'Payments_06_InvoicesPaid_Label';

                    UPDATE form_questions
                    SET options = " + payments06PercentageOptionsJson + @"
                    WHERE title = 'Payments_06_PctPaidWithin30Days';

                    UPDATE form_questions
                    SET options = " + payments06PercentageOptionsJson + @"
                    WHERE title = 'Payments_06_PctPaid31To60Days';

                    UPDATE form_questions
                    SET options = " + payments06PercentageOptionsJson + @"
                    WHERE title = 'Payments_06_PctPaid61OrMoreDays';

                    UPDATE form_questions
                    SET options = " + payments06PercentageOptionsJson + @"
                    WHERE title = 'Payments_06_PctPaidOverdue';

                    UPDATE form_questions
                    SET options = " + payments07GroupedOptionsJson + @"
                    WHERE title = 'Payments_07_Title';

                    UPDATE form_questions
                    SET options = " + payments07GroupedOptionsJson + @"
                    WHERE title = 'Payments_07_ReportingStartDate';

                    UPDATE form_questions
                    SET options = " + payments07DaysOptionsJson + @"
                    WHERE title = 'Payments_07_AverageDaysToPayInvoice';

                    UPDATE form_questions
                    SET options = " + payments07GroupedOptionsJson + @"
                    WHERE title = 'Payments_07_InvoicesPaid_Label';

                    -- Update Payments_07 percentage questions with special formatting
                    UPDATE form_questions
                    SET options = " + payments07PercentageOptionsJson + @"
                    WHERE title = 'Payments_07_PctPaidWithin30Days';

                    UPDATE form_questions
                    SET options = " + payments07PercentageOptionsJson + @"
                    WHERE title = 'Payments_07_PctPaid31To60Days';

                    UPDATE form_questions
                    SET options = " + payments07PercentageOptionsJson + @"
                    WHERE title = 'Payments_07_PctPaid61OrMoreDays';

                    UPDATE form_questions
                    SET options = " + payments07PercentageOptionsJson + @"
                    WHERE title = 'Payments_07_PctPaidOverdue';

                    -- Update Group 4 questions (Payments_08-11) with grouping
                    UPDATE form_questions
                    SET options = " + payments04GroupedOptionsJson + @"
                    WHERE title = 'Payments_08_Title';

                    UPDATE form_questions
                    SET options = " + payments04GroupedOptionsJson + @"
                    WHERE title = 'Payments_09_Title';

                    UPDATE form_questions
                    SET options = " + payments04GroupedOptionsJson + @"
                    WHERE title = 'Payments_10_Title';

                    UPDATE form_questions
                    SET options = " + payments11OptionsJson + @"
                    WHERE title = 'Payments_11_Title';

                    -- Change Payments_11_Title to file upload type (type 2)
                    UPDATE form_questions
                    SET type = 2
                    WHERE title = 'Payments_11_Title';

                    -- Fix sort order for Payments_06 questions to ensure correct display order
                    UPDATE form_questions SET sort_order = 7 WHERE title = 'Payments_06_ReportingStartDate';
                    UPDATE form_questions SET sort_order = 8 WHERE title = 'Payments_06_AverageDaysToPayInvoice';
                    UPDATE form_questions SET sort_order = 9 WHERE title = 'Payments_06_InvoicesPaid_Label';
                    UPDATE form_questions SET sort_order = 10 WHERE title = 'Payments_06_PctPaidWithin30Days';
                    UPDATE form_questions SET sort_order = 11 WHERE title = 'Payments_06_PctPaid31To60Days';
                    UPDATE form_questions SET sort_order = 12 WHERE title = 'Payments_06_PctPaid61OrMoreDays';
                    UPDATE form_questions SET sort_order = 13 WHERE title = 'Payments_06_PctPaidOverdue';

                    -- Fix sort order for Payments_07 questions to ensure correct display order
                    UPDATE form_questions SET sort_order = 15 WHERE title = 'Payments_07_ReportingStartDate';
                    UPDATE form_questions SET sort_order = 16 WHERE title = 'Payments_07_AverageDaysToPayInvoice';
                    UPDATE form_questions SET sort_order = 17 WHERE title = 'Payments_07_InvoicesPaid_Label';
                    UPDATE form_questions SET sort_order = 18 WHERE title = 'Payments_07_PctPaidWithin30Days';
                    UPDATE form_questions SET sort_order = 19 WHERE title = 'Payments_07_PctPaid31To60Days';
                    UPDATE form_questions SET sort_order = 20 WHERE title = 'Payments_07_PctPaid61OrMoreDays';
                    UPDATE form_questions SET sort_order = 21 WHERE title = 'Payments_07_PctPaidOverdue';

                    -- Fix incorrect question type for Payments_07_Title (should be type 0, not type 1)
                    UPDATE form_questions SET type = 0 WHERE title = 'Payments_07_Title';

                    -- Set caption for Payments_05
                    UPDATE form_questions SET caption = 'Payments_05_Caption' WHERE title = 'Payments_05_Title';

                    -- Set captions for Payments_06 and Payments_07
                    UPDATE form_questions SET caption = 'Payments_06_Caption' WHERE title = 'Payments_06_Title';
                    UPDATE form_questions SET caption = 'Payments_07_Caption' WHERE title = 'Payments_07_Title';

                    -- Set blank hint captions for date questions
                    UPDATE form_questions SET caption = '<span class=""govuk-!-display-none""></span>' WHERE title = 'Payments_06_ReportingStartDate';
                    UPDATE form_questions SET caption = '<span class=""govuk-!-display-none""></span>' WHERE title = 'Payments_07_ReportingStartDate';

                    -- Set summary titles for Group 2 (Payments_06) questions
                    UPDATE form_questions SET summary_title = 'Payments_06_ReportingStartDate_SummaryTitle' WHERE title = 'Payments_06_ReportingStartDate';
                    UPDATE form_questions SET summary_title = 'Payments_06_AverageDaysToPayInvoice_SummaryTitle' WHERE title = 'Payments_06_AverageDaysToPayInvoice';
                    UPDATE form_questions SET summary_title = 'Payments_06_PctPaidWithin30Days_SummaryTitle' WHERE title = 'Payments_06_PctPaidWithin30Days';
                    UPDATE form_questions SET summary_title = 'Payments_06_PctPaid31To60Days_SummaryTitle' WHERE title = 'Payments_06_PctPaid31To60Days';
                    UPDATE form_questions SET summary_title = 'Payments_06_PctPaid61OrMoreDays_SummaryTitle' WHERE title = 'Payments_06_PctPaid61OrMoreDays';
                    UPDATE form_questions SET summary_title = 'Payments_06_PctPaidOverdue_SummaryTitle' WHERE title = 'Payments_06_PctPaidOverdue';

                    -- Set summary titles for Group 3 (Payments_07) questions
                    UPDATE form_questions SET summary_title = 'Payments_07_ReportingStartDate_SummaryTitle' WHERE title = 'Payments_07_ReportingStartDate';
                    UPDATE form_questions SET summary_title = 'Payments_07_AverageDaysToPayInvoice_SummaryTitle' WHERE title = 'Payments_07_AverageDaysToPayInvoice';
                    UPDATE form_questions SET summary_title = 'Payments_07_PctPaidWithin30Days_SummaryTitle' WHERE title = 'Payments_07_PctPaidWithin30Days';
                    UPDATE form_questions SET summary_title = 'Payments_07_PctPaid31To60Days_SummaryTitle' WHERE title = 'Payments_07_PctPaid31To60Days';
                    UPDATE form_questions SET summary_title = 'Payments_07_PctPaid61OrMoreDays_SummaryTitle' WHERE title = 'Payments_07_PctPaid61OrMoreDays';
                    UPDATE form_questions SET summary_title = 'Payments_07_PctPaidOverdue_SummaryTitle' WHERE title = 'Payments_07_PctPaidOverdue';

                    -- Set summary title for Payments_08
                    UPDATE form_questions SET summary_title = 'Payments_08_SummaryTitle' WHERE title = 'Payments_08_Title';

                    -- Fix navigation flow for Payments_10 and Payments_11
                    DECLARE
                        payments10_id INT;
                        payments11_id INT;
                        checkyouranswers_id INT;
                    BEGIN
                        -- Get the question IDs for navigation updates
                        SELECT id INTO payments10_id FROM form_questions WHERE title = 'Payments_10_Title';
                        SELECT id INTO payments11_id FROM form_questions WHERE title = 'Payments_11_Title';
                        SELECT id INTO checkyouranswers_id FROM form_questions WHERE title = 'Global_CheckYourAnswers' AND section_id = (SELECT section_id FROM form_questions WHERE title = 'Payments_10_Title');

                        -- Update Payments_10_Title navigation: nextquestionid = check your answers, nextquestionalternativeid = Payments_11_Title
                        UPDATE form_questions
                        SET next_question_id = checkyouranswers_id,
                            next_question_alternative_id = payments11_id
                        WHERE title = 'Payments_10_Title';

                        -- Update Payments_11_Title navigation: nextquestionid = check your answers
                        UPDATE form_questions
                        SET next_question_id = checkyouranswers_id
                        WHERE title = 'Payments_11_Title';
                    END;

                END $$;
            ";

            migrationBuilder.Sql(sql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var sql = @"
                DO $$
                BEGIN
                    UPDATE form_questions
                    SET options = '{}'
                    WHERE title IN (
                        'Payments_02_Title',
                        'Payments_03_Title',
                        'Payments_04_Title',
                        'Payments_05_Title',
                        'Payments_06_Title',
                        'Payments_06_ReportingStartDate',
                        'Payments_06_AverageDaysToPayInvoice',
                        'Payments_06_InvoicesPaid_Label',
                        'Payments_06_PctPaidWithin30Days',
                        'Payments_06_PctPaid31To60Days',
                        'Payments_06_PctPaid61OrMoreDays',
                        'Payments_06_PctPaidOverdue',
                        'Payments_07_Title',
                        'Payments_07_ReportingStartDate',
                        'Payments_07_AverageDaysToPayInvoice',
                        'Payments_07_InvoicesPaid_Label',
                        'Payments_07_PctPaidWithin30Days',
                        'Payments_07_PctPaid31To60Days',
                        'Payments_07_PctPaid61OrMoreDays',
                        'Payments_07_PctPaidOverdue',
                        'Payments_08_Title',
                        'Payments_09_Title',
                        'Payments_10_Title',
                        'Payments_11_Title'
                    );

                    -- Revert Payments_11_Title type back to original (type 10)
                    UPDATE form_questions
                    SET type = 10
                    WHERE title = 'Payments_11_Title';

                    -- Revert caption for Payments_05
                    UPDATE form_questions SET caption = NULL WHERE title = 'Payments_05_Title';

                    -- Revert captions for Payments_06 and Payments_07
                    UPDATE form_questions SET caption = NULL WHERE title = 'Payments_06_Title';
                    UPDATE form_questions SET caption = NULL WHERE title = 'Payments_07_Title';

                    -- Revert blank hint captions for date questions
                    UPDATE form_questions SET caption = NULL WHERE title = 'Payments_06_ReportingStartDate';
                    UPDATE form_questions SET caption = NULL WHERE title = 'Payments_07_ReportingStartDate';

                    -- Revert summary titles for all updated questions
                    UPDATE form_questions SET summary_title = NULL WHERE title IN (
                        'Payments_06_ReportingStartDate',
                        'Payments_06_AverageDaysToPayInvoice',
                        'Payments_06_PctPaidWithin30Days',
                        'Payments_06_PctPaid31To60Days',
                        'Payments_06_PctPaid61OrMoreDays',
                        'Payments_06_PctPaidOverdue',
                        'Payments_07_ReportingStartDate',
                        'Payments_07_AverageDaysToPayInvoice',
                        'Payments_07_PctPaidWithin30Days',
                        'Payments_07_PctPaid31To60Days',
                        'Payments_07_PctPaid61OrMoreDays',
                        'Payments_07_PctPaidOverdue',
                        'Payments_08_Title'
                    );

                    -- Revert navigation flow for Payments_10 and Payments_11
                    DECLARE
                        payments11_id INT;
                    BEGIN
                        -- Get the question ID for navigation reversion
                        SELECT id INTO payments11_id FROM form_questions WHERE title = 'Payments_11_Title';

                        -- Revert Payments_10_Title navigation: nextquestionid = Payments_11_Title, nextquestionalternativeid = NULL
                        UPDATE form_questions
                        SET next_question_id = payments11_id,
                            next_question_alternative_id = NULL
                        WHERE title = 'Payments_10_Title';

                        -- Payments_11_Title navigation remains the same (nextquestionid = check your answers)
                        -- This doesn't need to be reverted as it was already correct
                    END;
                END $$;
            ";

            migrationBuilder.Sql(sql);
        }
    }
}

