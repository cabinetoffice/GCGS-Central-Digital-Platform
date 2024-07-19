using CO.CDP.OrganisationInformation.Persistence.Forms;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CreateFinancialInformationSeedDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var formsGuid = Guid.Parse("0618b13e-eaf2-46e3-a7d2-6f2c44be7022");
            var sectonGuid = Guid.Parse("13511cb1-9ed4-4d72-ba9e-05b4a0be880c");

            migrationBuilder.Sql($@"
                INSERT INTO forms (guid, name, version, is_required, type, scope)
                VALUES ('{formsGuid}', 'Financial Information Form', '1.0', TRUE, 0, 0);
            ");

            migrationBuilder.Sql($@"
                DO $$
                DECLARE
                    formId INT;
                BEGIN
                    SELECT currval(pg_get_serial_sequence('forms', 'id')) INTO formId;

                    -- Insert into form_sections
                    INSERT INTO form_sections (guid, title, form_id)
                    VALUES ('{sectonGuid}', 'Financial Information', formId);

                    -- Retrieve the inserted form_section ID
                    SELECT currval(pg_get_serial_sequence('form_sections', 'id')) INTO formId;

                    -- Insert into form_questions
                    INSERT INTO form_questions (guid, section_id, type, is_required, title, description, options)
                    VALUES
        ('{Guid.NewGuid()}', formId, {(int)FormQuestionType.NoInput}, TRUE, 'The financial information you will need.', 'You will need to upload accounts or statements for your 2 most recent financial years. If you do not have 2 years, you can upload your most recent financial year. You will need to enter the financial year end date for the information you upload.', '{{}}'),
        ('{Guid.NewGuid()}', formId, {(int)FormQuestionType.YesOrNo}, TRUE, 'Were your accounts audited?', NULL, '{{}}'),
        ('{Guid.NewGuid()}', formId, {(int)FormQuestionType.FileUpload}, TRUE, 'Upload your accounts', 'Upload your most recent 2 financial years. If you do not have 2, upload your most recent financial year.', '{{}}'),
        ('{Guid.NewGuid()}', formId, {(int)FormQuestionType.Date}, TRUE, 'What is the financial year end date for the information you uploaded?', NULL, '{{}}'),
        ('{Guid.NewGuid()}', formId, {(int)FormQuestionType.Text}, TRUE, 'Enter a description of your financial status.', 'Please provide detailed information about your financial status.', '{{}}'),
        ('{Guid.NewGuid()}', formId, {(int)FormQuestionType.SingleChoice}, TRUE, 'Were your accounts audited?.', NULL, '{{""choices"": [{{""id"": ""2687c252-236d-4804-b3a4-e3be396aa949"", ""title"": ""Yes"", ""groupName"": null, ""hint"": null, ""value"": ""Yes""}}, {{""id"": ""a7167886-0017-4f9d-978b-a845bacac9f9"", ""title"": ""No"", ""groupName"": null, ""hint"": null, ""value"": ""No""}}], ""choiceProviderStrategy"": null}}'),
        ('{Guid.NewGuid()}', formId, {(int)FormQuestionType.MultipleChoice}, TRUE, 'Which of the following statements applies to your accounts or financial statements?.', NULL, '{{""choices"": [{{""id"": ""df7f9ab0-7b59-4fee-8dd7-74fde0348039"", ""title"": ""Accounts submitted for our last two financial years were required to be audited."", ""groupName"": null, ""hint"": {{""title"": null, ""description"": ""This means your accounts were audited for the last two years.""}}, ""value"": ""accountsLastTwoYearsAudited""}}, {{""id"": ""cf94e70b-aa5e-457f-bfcd-bf5f3150e688"", ""title"": ""Accounts submitted for only our most recent financial year were required to be audited."", ""groupName"": null, ""hint"": {{""title"": null, ""description"": ""This means your accounts were audited only for the most recent year.""}}, ""value"": ""accountsRecentYearAudited""}}, {{""id"": ""b97d4dd8-1437-4ddb-8778-d6807de558ff"", ""title"": ""Financial information for our most recent financial year. Our accounts did not require an audit."", ""groupName"": null, ""hint"": {{""title"": null, ""description"": ""This means your accounts were not audited for the most recent year.""}}, ""value"": ""recentYearNotAudited""}}], ""choiceProviderStrategy"": null}}'),
        ('{Guid.NewGuid()}', formId, {(int)FormQuestionType.CheckYourAnswers}, TRUE, 'Check your answers', NULL, '{{}}');
    END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("TRUNCATE TABLE form_questions CASCADE;");
            migrationBuilder.Sql("TRUNCATE TABLE form_sections CASCADE;");
            migrationBuilder.Sql("TRUNCATE TABLE forms CASCADE;");
        }
    }
}
