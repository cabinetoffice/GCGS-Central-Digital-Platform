using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PaymentsSectionForm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                DO $$
                DECLARE
                    v_form_id int;
                    v_section_id INT;
                    v_question1_id INT;
                    v_question2_id INT;
                    v_question3_id INT;
                    v_question4_id INT;
                    v_question5_id INT;
                    v_question6_id INT;
                    v_question7_id INT;
                    v_question8_id INT;
                    v_question9_id INT;
                    v_question10_id INT;
                    v_question11_id INT;
                    v_question12_id INT;
                    v_question6_1_id INT;
                    v_question6_2_id INT;
                    v_question6_3_id INT;
                    v_question6_4_id INT;
                    v_question6_5_id INT;
                    v_question6_6_id INT;
                    v_question6_7_id INT;
                    v_question7_1_id INT;
                    v_question7_2_id INT;
                    v_question7_3_id INT;
                    v_question7_4_id INT;
                    v_question7_5_id INT;
                    v_question7_6_id INT;
                    v_question7_7_id INT;
                BEGIN
                    SELECT id INTO v_form_id FROM forms WHERE name = 'Standard Questions';

                    IF v_form_id IS NULL THEN
                        RAISE EXCEPTION 'Form ""Standard Questions"" not found. Migration cannot proceed.';
                    END IF;

	                INSERT INTO form_sections (guid, title, form_id, allows_multiple_answer_sets, check_further_questions_exempted, type,display_order, configuration)
                    VALUES ('{Guid.NewGuid()}', 'Payments_SectionTitle', v_form_id, FALSE, FALSE, 3,1, '{{}}')
                    RETURNING id INTO v_section_id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"", caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 0, true, 'Payments_01_Title', 'Payments_01_Description', '{{}}', NULL, '', '_Payments01', 1)
                    RETURNING id INTO v_question1_id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"", caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 3, true, 'Payments_02_Title', 'Payments_02_Description', '{{}}', NULL, 'Payments_02_SummaryTitle', '_Payments02', 2)
                    RETURNING id INTO v_question2_id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"", caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 3, true, 'Payments_03_Title', 'Payments_03_Description', '{{}}', NULL, 'Payments_03_SummaryTitle', '_Payments03', 3)
                    RETURNING id INTO v_question3_id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"", caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 3, true, 'Payments_04_Title', 'Payments_04_Description', '{{}}', NULL, 'Payments_04_SummaryTitle', '_Payments04', 4)
                    RETURNING id INTO v_question4_id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"", caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 12, false, 'Payments_05_Title', NULL, '{{}}', NULL, 'Payments_05_SummaryTitle', '_Payments05', 5)
                    RETURNING id INTO v_question5_id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 0, true, 'Payments_06_Title', 'Payments_06_Description', '{{}}', NULL, '', '_Payments06', 6)
                    RETURNING id INTO v_question6_id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 7, true, 'Payments_06_ReportingStartDate', null, '{{}}', NULL, '', '_Payments06_1', 7)
                    RETURNING id INTO v_question6_1_id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 1, true, 'Payments_06_AverageDaysToPayInvoice', null, '{{}}', NULL, '', '_Payments06_2', 8)
                    RETURNING id INTO v_question6_2_id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 0, true, 'Payments_06_InvoicesPaid_Label', null, '{{}}', NULL, '', '_Payments06_3', 9)
                    RETURNING id INTO v_question6_3_id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 1, true, 'Payments_06_PctPaidWithin30Days', null, '{{}}', NULL, '', '_Payments06_4', 10)
                    RETURNING id INTO v_question6_4_id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 1, true, 'Payments_06_PctPaid31To60Days', null, '{{}}', NULL, '', '_Payments06_5', 11)
                    RETURNING id INTO v_question6_5_id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 1, true, 'Payments_06_PctPaid61OrMoreDays', null, '{{}}', NULL, '', '_Payments06_6', 12)
                    RETURNING id INTO v_question6_6_id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 1, true, 'Payments_06_PctPaidOverdue', null, '{{}}', NULL, '', '_Payments06_7', 13)
                    RETURNING id INTO v_question6_7_id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"", caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 1, true, 'Payments_07_Title', 'Payments_07_Description', '{{}}', NULL, '', '_Payments07', 14)
                    RETURNING id INTO v_question7_id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 7, true, 'Payments_07_ReportingStartDate', null, '{{}}', NULL, '', '_Payments07_1', 15)
                    RETURNING id INTO v_question7_1_id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 1, true, 'Payments_07_AverageDaysToPayInvoice', null, '{{}}', NULL, '', '_Payments07_2', 16)
                    RETURNING id INTO v_question7_2_id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 0, true, 'Payments_07_InvoicesPaid_Label', null, '{{}}', NULL, '', '_Payments07_3', 17)
                    RETURNING id INTO v_question7_3_id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 1, true, 'Payments_07_PctPaidWithin30Days', null, '{{}}', NULL, '', '_Payments07_4', 18)
                    RETURNING id INTO v_question7_4_id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 1, true, 'Payments_07_PctPaid31To60Days', null, '{{}}', NULL, '', '_Payments07_5', 19)
                    RETURNING id INTO v_question7_5_id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 1, true, 'Payments_07_PctPaid61OrMoreDays', null, '{{}}', NULL, '', '_Payments07_6', 20)
                    RETURNING id INTO v_question7_6_id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 1, true, 'Payments_07_PctPaidOverdue', null, '{{}}', NULL, '', '_Payments07_7', 21)
                    RETURNING id INTO v_question7_7_id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 10, true, 'Payments_08_Title', 'Payments_08_Description', '{{}}', NULL, '', '_Payments08', 22)
                    RETURNING id INTO v_question8_id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 3, true, 'Payments_09_Title', 'Payments_09_Description', '{{}}', NULL, 'Payments_09_SummaryTitle', '_Payments09', 23)
                    RETURNING id INTO v_question9_id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 3, true, 'Payments_10_Title', 'Payments_10_Description', '{{}}', NULL, 'Payments_10_SummaryTitle', '_Payments10', 24)
                    RETURNING id INTO v_question10_id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"",  caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 10, true, 'Payments_11_Title', 'Payments_11_Description', '{{}}', NULL, 'Payments_11_SummaryTitle', '_Payments11', 25)
                    RETURNING id INTO v_question11_id;

                    INSERT INTO form_questions (guid, next_question_id, next_question_alternative_id, section_id, ""type"", is_required, title, description, ""options"", caption, summary_title, ""name"", sort_order)
                    VALUES('{Guid.NewGuid()}', NULL, NULL, v_section_id, 6, true, 'Global_CheckYourAnswers', NULL, '{{}}', NULL, '', '_Payments12', 26)
                    RETURNING id INTO v_question12_id;

                    -- Update the next question links
                    UPDATE form_questions SET next_question_id = v_question2_id WHERE title = 'Payments_01_Title' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question3_id WHERE title = 'Payments_02_Title' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question4_id WHERE title = 'Payments_03_Title' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question5_id WHERE title = 'Payments_04_Title' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question6_id WHERE title = 'Payments_05_Title' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question6_1_id WHERE title = 'Payments_06_Title' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question6_2_id WHERE title = 'Payments_06_ReportingStartDate' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question6_3_id WHERE title = 'Payments_06_AverageDaysToPayInvoice' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question6_4_id WHERE title = 'Payments_06_InvoicesPaid_Label' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question6_5_id WHERE title = 'Payments_06_PctPaidWithin30Days' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question6_6_id WHERE title = 'Payments_06_PctPaid31To60Days' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question6_7_id WHERE title = 'Payments_06_PctPaid61OrMoreDays' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question7_id WHERE title = 'Payments_06_PctPaidOverdue' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question7_1_id WHERE title = 'Payments_07_Title' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question7_2_id WHERE title = 'Payments_07_ReportingStartDate' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question7_3_id WHERE title = 'Payments_07_AverageDaysToPayInvoice' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question7_4_id WHERE title = 'Payments_07_InvoicesPaid_Label' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question7_5_id WHERE title = 'Payments_07_PctPaidWithin30Days' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question7_6_id WHERE title = 'Payments_07_PctPaid31To60Days' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question7_7_id WHERE title = 'Payments_07_PctPaid61OrMoreDays' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question8_id WHERE title = 'Payments_07_PctPaidOverdue' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question9_id WHERE title = 'Payments_08_Title' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question10_id WHERE title = 'Payments_09_Title' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question11_id WHERE title = 'Payments_10_Title' AND section_id = v_section_id;
                    UPDATE form_questions SET next_question_id = v_question12_id WHERE title = 'Payments_11_Title' AND section_id = v_section_id;

                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                DO $$
                DECLARE
                    v_section_id INTEGER;
                BEGIN
                    -- Get the ID of the section for Payments
                    SELECT id INTO v_section_id FROM form_sections WHERE title = 'Payments_SectionTitle';

                    IF v_section_id IS NOT NULL THEN
                        -- Delete the questions associated with the section first
                        DELETE FROM form_questions WHERE section_id = v_section_id;
                        -- Delete the Payments section
                        DELETE FROM form_sections WHERE id = v_section_id;
                    END IF;
                END $$;
            ");
        }
    }
}
