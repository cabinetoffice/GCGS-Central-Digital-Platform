using CO.CDP.OrganisationInformation.Persistence.Forms;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CreateShareCodesSeedDate : Migration
    {
        private readonly Guid formsGuid = Guid.Parse("049df96e-f6ea-423c-9361-ecb20c0250ea");
        private readonly Guid sectionGuid = Guid.Parse("936096b3-c3bb-4475-ad7d-73b44ff61e76");

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var formsGuid = Guid.Parse("049df96e-f6ea-423c-9361-ecb20c0250ea");
            var sectionGuid = Guid.Parse("936096b3-c3bb-4475-ad7d-73b44ff61e76");

            migrationBuilder.Sql($@"
                INSERT INTO forms (guid, name, version, is_required, type, scope)
                VALUES ('{formsGuid}', 'Declaration Information Form', '1.0', TRUE, 0, 0);
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

                    INSERT INTO form_sections (guid, title, form_id)
                    VALUES ('{sectionGuid}', 'Declaration Information', formId);

                    SELECT currval(pg_get_serial_sequence('form_sections', 'id')) INTO sectionId;

                    INSERT INTO form_questions (guid, section_id, type, next_question_id, is_required, title, description, options)
                    VALUES ('{Guid.NewGuid()}', sectionId, {(int)FormQuestionType.Text}, NULL, TRUE, 'Enter your name', 'Your name as the person authorised to declare the supplier information.', '{{}}'
                    );

                    SELECT currval(pg_get_serial_sequence('form_questions', 'id')) INTO previousQuestionId;

                    INSERT INTO form_questions (guid, section_id, type, next_question_id, is_required, title, description, options)
                    VALUES ('{Guid.NewGuid()}', sectionId, {(int)FormQuestionType.Text}, previousQuestionId, TRUE, 'Enter your job title', NULL, '{{}}'
                    );

                    SELECT currval(pg_get_serial_sequence('form_questions', 'id')) INTO previousQuestionId;

                    INSERT INTO form_questions (guid, section_id, type, next_question_id, is_required, title, description, options)
                    VALUES ('{Guid.NewGuid()}', sectionId, {(int)FormQuestionType.Text}, previousQuestionId, TRUE, 'Enter your email address', 'So the contracting authority can contact you.', '{{}}'
                    );

                    SELECT currval(pg_get_serial_sequence('form_questions', 'id')) INTO previousQuestionId;

                    INSERT INTO form_questions (guid, section_id, type, next_question_id, is_required, title, description, options)
                    VALUES ('{Guid.NewGuid()}', sectionId, {(int)FormQuestionType.Address}, previousQuestionId, TRUE, 'Enter your postal address', 'So the contracting authority can contact you.', '{{""choices"": [{{""id"": ""bd4fe649-1f52-429e-978c-472e0a2cf11c"", ""title"": ""Address line 1"", ""groupName"": null, ""hint"": {{""title"": null, ""description"": ""Enter the first line of your address.""}}, ""value"": ""addressLine1""}}, {{""id"": ""a4edc9cd-f198-4e74-88e4-d981b09d30ed"", ""title"": ""Town or city"", ""groupName"": null, ""hint"": {{""title"": null, ""description"": ""Enter the name of your town or city.""}}, ""value"": ""townOrCity""}}, {{""id"": ""ef806205-6e38-4496-9a3e-8bb7a37b48ba"", ""title"": ""Post code"", ""groupName"": null, ""hint"": {{""title"": null, ""description"": ""Enter your postal code.""}}, ""value"": ""postCode""}}], ""choiceProviderStrategy"": null}}'
                    );

                    SELECT currval(pg_get_serial_sequence('form_questions', 'id')) INTO previousQuestionId;

                    INSERT INTO form_questions (guid, section_id, type, next_question_id, is_required, title, description, options)
                    VALUES ('{Guid.NewGuid()}', sectionId, {(int)FormQuestionType.CheckYourAnswers}, previousQuestionId, TRUE, 'Check your answers', NULL, '{{}}'
                    );
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                DELETE FROM form_questions WHERE section_id IN (SELECT id FROM form_sections WHERE form_id = (SELECT id FROM forms WHERE guid = '{formsGuid}'));
                DELETE FROM form_sections WHERE form_id = (SELECT id FROM forms WHERE guid = '{formsGuid}');
                DELETE FROM forms WHERE guid = '{formsGuid}';
            ");
        }
    }
}
