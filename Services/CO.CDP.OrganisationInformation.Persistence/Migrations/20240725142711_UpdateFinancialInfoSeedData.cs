using Microsoft.EntityFrameworkCore.Migrations;
using CO.CDP.OrganisationInformation.Persistence.Forms;
#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFinancialInfoSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("TRUNCATE TABLE form_questions CASCADE;");
            migrationBuilder.Sql("TRUNCATE TABLE form_sections CASCADE;");
            migrationBuilder.Sql("TRUNCATE TABLE forms CASCADE;");

            var formsGuid = Guid.Parse("0618b13e-eaf2-46e3-a7d2-6f2c44be7022");
            var sectionGuid = Guid.Parse("13511cb1-9ed4-4d72-ba9e-05b4a0be880c");
            var noInputGuid = Guid.Parse("c1e2e3f4-5a6b-7c8d-9e0f-123456789abc");
            var yesOrNoGuid = Guid.Parse("d2e3f4a5-6b7c-8d9e-0f1a-23456789bcd0");
            var fileUploadGuid = Guid.Parse("e3f4a5b6-7c8d-9e0f-1a2b-3456789cdef1");
            var dateGuid = Guid.Parse("f4a5b6c7-8d9e-0f1a-2b3c-456789def012");
            var checkYourAnswersGuid = Guid.Parse("a5b6c7d8-9e0f-1a2b-3c4d-56789ef01234");

            migrationBuilder.Sql($@"
                INSERT INTO forms (guid, name, version, is_required, type, scope)
                VALUES ('{formsGuid}', 'Financial Information Form', '1.0', TRUE, 0, 0);
            ");

            migrationBuilder.Sql($@"
                DO $$
                DECLARE
                    formId INT;
                    sectionId INT;
                    questionId INT;
                    previousQuestionId INT;
                BEGIN
                    SELECT currval(pg_get_serial_sequence('forms', 'id')) INTO formId;

                    -- Insert into form_sections
                    INSERT INTO form_sections (guid, title, form_id)
                    VALUES ('{sectionGuid}', 'Financial Information', formId);

                    -- Retrieve the inserted form_section ID
                    SELECT currval(pg_get_serial_sequence('form_sections', 'id')) INTO sectionId;

                    -- Insert fifth question (CheckYourAnswers)
                    INSERT INTO form_questions (guid, section_id, type, next_question_id, is_required, title, description, options)
                    VALUES ('{checkYourAnswersGuid}', sectionId, {(int)FormQuestionType.CheckYourAnswers}, NULL, TRUE, 'Check your answers', NULL, '{{}}');

                    -- Retrieve the inserted question ID
                    SELECT currval(pg_get_serial_sequence('form_questions', 'id')) INTO previousQuestionId;

                    -- Insert fourth question (Date)
                    INSERT INTO form_questions (guid, section_id, type, next_question_id, is_required, title, description, options)
                    VALUES ('{dateGuid}', sectionId, {(int)FormQuestionType.Date}, previousQuestionId, TRUE, 'What is the financial year end date for the information you uploaded?', NULL, '{{}}');

                    -- Retrieve the inserted question ID
                    SELECT currval(pg_get_serial_sequence('form_questions', 'id')) INTO previousQuestionId;

                    -- Insert third question (FileUpload)
                    INSERT INTO form_questions (guid, section_id, type, next_question_id, is_required, title, description, options)
                    VALUES ('{fileUploadGuid}', sectionId, {(int)FormQuestionType.FileUpload}, previousQuestionId, TRUE, 'Upload your accounts', '<p class=""govuk-body"">Upload your most recent 2 financial years. If you do not have 2, upload your most recent financial year.</p>', '{{}}');

                    -- Retrieve the inserted question ID
                    SELECT currval(pg_get_serial_sequence('form_questions', 'id')) INTO previousQuestionId;

                    -- Insert second question (YesOrNo)
                    INSERT INTO form_questions (guid, section_id, type, next_question_id, is_required, title, description, options)
                    VALUES ('{yesOrNoGuid}', sectionId, {(int)FormQuestionType.YesOrNo}, previousQuestionId, TRUE, 'Were your accounts audited?', NULL, '{{}}');

                    -- Retrieve the inserted question ID
                    SELECT currval(pg_get_serial_sequence('form_questions', 'id')) INTO previousQuestionId;

                    -- Insert first question (NoInput)
                    INSERT INTO form_questions (guid, section_id, type, next_question_id, is_required, title, description, options)
                    VALUES ('{noInputGuid}', sectionId, {(int)FormQuestionType.NoInput}, previousQuestionId, TRUE, 'The financial information you will need.', '<p class=""govuk-body"">You will need to upload accounts or statements for your 2 most recent financial years.</p><p class=""govuk-body"">If you do not have 2 years, you can upload your most recent financial year.</p><p class=""govuk-body"">You will need to enter the financial year end date for the information you upload.</p>', '{{}}');
                END $$;
            ");

            migrationBuilder.Sql($@"
                UPDATE form_sections
                SET configuration = '{{""AddAnotherAnswerLabel"": ""Add another file?"", ""SingularSummaryHeading"": ""You have added 1 file"", ""RemoveConfirmationCaption"": ""Economic and Financial Standing"", ""RemoveConfirmationHeading"": ""Are you sure you want to remove this file?"", ""PluralSummaryHeadingFormat"": ""You have added {{0}} files""}}'
                WHERE guid = '13511cb1-9ed4-4d72-ba9e-05b4a0be880c';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                UPDATE form_sections
                SET configuration = '{{}}'
                WHERE guid = '13511cb1-9ed4-4d72-ba9e-05b4a0be880c';
            ");
        }
    }
}