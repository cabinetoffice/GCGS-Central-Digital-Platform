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
            migrationBuilder.Sql($@"
                INSERT INTO forms (guid, name, version, is_required, type, scope)
                VALUES ('{Guid.NewGuid()}', 'Financial Information Form', '1.0', TRUE, 0, 0);
            ");

            migrationBuilder.Sql($@"
                DO $$
                DECLARE
                    formId INT;
                BEGIN
                    SELECT currval(pg_get_serial_sequence('forms', 'id')) INTO formId;

                    -- Insert into form_sections
                    INSERT INTO form_sections (guid, title, form_id)
                    VALUES ('{Guid.NewGuid()}', 'Financial Information', formId);

                    -- Retrieve the inserted form_section ID
                    SELECT currval(pg_get_serial_sequence('form_sections', 'id')) INTO formId;

                    -- Insert into form_questions
                    INSERT INTO form_questions (guid, section_id, type, is_required, title, description, options)
                    VALUES
                    ('{Guid.NewGuid()}', formId, {(int)FormQuestionType.NoInput}, TRUE, 'The financial information you will need.', 'You will need to upload accounts or statements for your 2 most recent financial years. If you do not have 2 years, you can upload your most recent financial year. You will need to enter the financial year end date for the information you upload.', '{{}}'),
                    ('{Guid.NewGuid()}', formId, {(int)FormQuestionType.YesOrNo}, TRUE, 'Were your accounts audited?', NULL, '{{}}'),
                    ('{Guid.NewGuid()}', formId, {(int)FormQuestionType.FileUpload}, TRUE, 'Upload your accounts', 'Upload your most recent 2 financial years. If you do not have 2, upload your most recent financial year.', '{{}}'),
                    ('{Guid.NewGuid()}', formId, {(int)FormQuestionType.Date}, TRUE, 'What is the financial year end date for the information you uploaded?', NULL, '{{}}');
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "form_questions",
                keyColumn: "Id",
                keyValues: new object[] { 1, 2, 3, 4 });

            migrationBuilder.DeleteData(
                table: "form_sections",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "forms",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
