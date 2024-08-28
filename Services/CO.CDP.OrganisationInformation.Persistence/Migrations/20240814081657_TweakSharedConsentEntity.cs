using System;
using CO.CDP.OrganisationInformation.Persistence.Forms;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TweakSharedConsentEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "submitted_at",
                table: "shared_consents",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "form_version_id",
                table: "shared_consents",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "booking_reference",
                table: "shared_consents",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");


            // add financial information flow truncated by other migrations
            var financialInformationFormsGuid = Guid.Parse("0618b13e-eaf2-46e3-a7d2-6f2c44be7022");
            var financialInformationSectionGuid = Guid.Parse("13511cb1-9ed4-4d72-ba9e-05b4a0be880c");

            var noInputGuid = Guid.Parse("c1e2e3f4-5a6b-7c8d-9e0f-123456789abc");
            var yesOrNoGuid = Guid.Parse("d2e3f4a5-6b7c-8d9e-0f1a-23456789bcd0");
            var fileUploadGuid = Guid.Parse("e3f4a5b6-7c8d-9e0f-1a2b-3456789cdef1");
            var dateGuid = Guid.Parse("f4a5b6c7-8d9e-0f1a-2b3c-456789def012");
            var checkYourAnswersGuid = Guid.Parse("a5b6c7d8-9e0f-1a2b-3c4d-56789ef01234");

            migrationBuilder.Sql($@"
                INSERT INTO forms (guid, name, version, is_required, type, scope)
                VALUES ('{financialInformationFormsGuid}', 'Financial Information Form', '1.0', TRUE, 0, 0);
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
                    VALUES ('{financialInformationSectionGuid}', 'Financial Information', formId);

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

            // add share code flow truncated by other migrations
            var shareCodesFormsGuid = Guid.Parse("049df96e-f6ea-423c-9361-ecb20c0250ea");
            var shareCodesSectionGuid = Guid.Parse("936096b3-c3bb-4475-ad7d-73b44ff61e76");

            migrationBuilder.Sql($@"
                INSERT INTO forms (guid, name, version, is_required, type, scope)
                VALUES ('{shareCodesFormsGuid}', 'Declaration Information Form', '1.0', TRUE, 0, 0);
            ");

            migrationBuilder.Sql($@"
                DO $$
                DECLARE
                    formId INT;
                    sectionId INT;
                    previousQuestionId INT;
                    declarationQuestionId INT;
                BEGIN
                    SELECT currval(pg_get_serial_sequence('forms', 'id')) INTO formId;

                    INSERT INTO form_sections (guid, title, form_id)
                    VALUES ('{shareCodesSectionGuid}', 'Declaration Information', formId);

                    SELECT currval(pg_get_serial_sequence('form_sections', 'id')) INTO sectionId;

                    INSERT INTO form_questions (guid, section_id, type, is_required, title, description, options)
                    VALUES ('{Guid.NewGuid()}', sectionId, {(int)FormQuestionType.CheckYourAnswers}, TRUE, 'Check your answers', NULL, '{{}}')
                    RETURNING id INTO previousQuestionId;

                    INSERT INTO form_questions (guid, section_id, type, next_question_id, is_required, title, caption, description, options)
                    VALUES ('{Guid.NewGuid()}', sectionId, {(int)FormQuestionType.Address}, previousQuestionId, TRUE, 'Enter your postal address', 'Your declaration details', '<div id=""declarationNameHint"" class=""govuk-hint"">So the contracting authority can contact you.</div>', '{{""choices"": [{{""id"": ""bd4fe649-1f52-429e-978c-472e0a2cf11c"", ""title"": ""Address line 1"", ""groupName"": null, ""hint"": {{""title"": null, ""description"": ""Enter the first line of your address.""}}, ""value"": ""addressLine1""}}, {{""id"": ""a4edc9cd-f198-4e74-88e4-d981b09d30ed"", ""title"": ""Town or city"", ""groupName"": null, ""hint"": {{""title"": null, ""description"": ""Enter the name of your town or city.""}}, ""value"": ""townOrCity""}}, {{""id"": ""ef806205-6e38-4496-9a3e-8bb7a37b48ba"", ""title"": ""Post code"", ""groupName"": null, ""hint"": {{""title"": null, ""description"": ""Enter your postal code.""}}, ""value"": ""postCode""}}], ""choiceProviderStrategy"": null}}')
                    RETURNING id INTO previousQuestionId;

                    INSERT INTO form_questions (guid, section_id, type, next_question_id, is_required, title, caption, description, options)
                    VALUES ('{Guid.NewGuid()}', sectionId, {(int)FormQuestionType.Text}, previousQuestionId, TRUE, 'Enter your email address', 'Your declaration details', '<div id=""declarationNameHint"" class=""govuk-hint"">So the contracting authority can contact you.</div>', '{{}}')
                    RETURNING id INTO previousQuestionId;

                    INSERT INTO form_questions (guid, section_id, type, next_question_id, is_required, title, caption, description, options)
                    VALUES ('{Guid.NewGuid()}', sectionId, {(int)FormQuestionType.Text}, previousQuestionId, TRUE, 'Enter your job title', 'Your declaration details', NULL, '{{}}')
                    RETURNING id INTO previousQuestionId;

                    INSERT INTO form_questions (guid, section_id, type, next_question_id, is_required, title, caption, description, options)
                    VALUES ('{Guid.NewGuid()}', sectionId, {(int)FormQuestionType.Text}, previousQuestionId, TRUE, 'Enter your name', 'Your declaration details', '<div id=""declarationNameHint"" class=""govuk-hint"">Your name as the person authorised to declare the supplier information.</div>', '{{}}')
                    RETURNING id INTO previousQuestionId;

                    INSERT INTO form_questions (guid, section_id, type, next_question_id, is_required, title, description, options)
                    VALUES ('{Guid.NewGuid()}', sectionId, {(int)FormQuestionType.CheckBox}, previousQuestionId, TRUE, 'Declaration', '<ul class=""govuk-list govuk-list--bullet""><li>I am authorised to make this declaration on behalf of the supplier and declare that to the best of my knowledge the answers submitted and information contained is correct and accurate at the time of declaration.</li><br><li>I declare that, upon request from the Contracting Authority and without delay I will provide the certificates or documentary evidence referred to in this information.</li><br><li>I understand that the information is required as per the regulations of the Procurement Act 2023 and may be used in the selection process to assess my suitability to participate further in this procurement.</li><br><li>I understand that a contracting authority with whom this information is shared may request further clarity or detail on information provided in this submission.</li></ul>', '{{""choices"": [{{""id"": ""bd4fe649-1f52-429e-978c-472e0a2cf11c"", ""title"": ""I understand and agree to the above statements"", ""groupName"": null, ""hint"": null, ""value"": ""agree""}}], ""choiceProviderStrategy"": null}}')
                    RETURNING id INTO declarationQuestionId;
                END $$;
            ");


            migrationBuilder.Sql($@"
                UPDATE form_sections
                SET configuration = '{{""AddAnotherAnswerLabel"": ""Add another file?"", ""SingularSummaryHeading"": ""You have added 1 file"", ""RemoveConfirmationCaption"": ""Economic and Financial Standing"", ""RemoveConfirmationHeading"": ""Are you sure you want to remove this file?"", ""PluralSummaryHeadingFormat"": ""You have added {{0}} files""}}'
                WHERE guid = '{financialInformationSectionGuid}';
            ");

            migrationBuilder.Sql($@"
                UPDATE form_questions
                SET summary_title = 'Audited accounts'
                WHERE title = 'Were your accounts audited?';

                UPDATE form_questions
                SET summary_title = 'Document uploaded'
                WHERE title = 'Upload your accounts';

                UPDATE form_questions
                SET summary_title = 'Date of financial year end'
                WHERE title = 'What is the financial year end date for the information you uploaded?';

                UPDATE form_questions
                SET summary_title = 'Declared by'
                WHERE title = 'Enter your name';

                UPDATE form_questions
                SET summary_title = 'Job title'
                WHERE title = 'Enter your job title';

                UPDATE form_questions
                SET summary_title = 'Email address'
                WHERE title = 'Enter your email address';

                UPDATE form_questions
                SET summary_title = 'Postal address'
                WHERE title = 'Enter your postal address';
            ");

            migrationBuilder.Sql($@"
                UPDATE form_questions
                SET caption = 'Your declaration details',
                description = '<div id=""declarationNameEmailHint"" class=""govuk-hint"">Your name as the person authorised to declare the supplier information.</div>'
                WHERE title = 'Enter your name';

                UPDATE form_questions
                SET caption = 'Your declaration details',
                description = null
                WHERE title = 'Enter your job title';

                UPDATE form_questions
                SET caption = 'Your declaration details',
                description = '<div id=""declarationNameEmailHint"" class=""govuk-hint"">So the contracting authority can contact you.</div>'
                WHERE title = 'Enter your email address';

                UPDATE form_questions
                SET caption = 'Your declaration details',
                description = '<div id=""declarationNameEmailHint"" class=""govuk-hint"">So the contracting authority can contact you.</div>'
                WHERE title = 'Enter your postal address';
            ");

            migrationBuilder.Sql($@"
                UPDATE form_sections
                SET allows_multiple_answer_sets = true
                WHERE guid = '{financialInformationSectionGuid}';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "submitted_at",
                table: "shared_consents",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "form_version_id",
                table: "shared_consents",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "booking_reference",
                table: "shared_consents",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}