using CO.CDP.OrganisationInformation.Persistence.Forms;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ConsortiumSharedConsentFormScript : Migration
    {
        private readonly Guid formsGuid = Guid.Parse("24482a2a-88a8-4432-b03c-4c966c9fce23");
        private readonly Guid shareCodesSectionGuid = Guid.Parse("13511cb1-9ed4-4d72-ba9e-05b4a0be880c");

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
            INSERT INTO forms (guid, name, version, is_required, scope)
            VALUES ('{formsGuid}', 'Consortium Information Form', '1.0', TRUE, 0);
        ");

            migrationBuilder.Sql($@"
            DO $$
            DECLARE
                formId INT;
                sectionId INT;
                previousQuestionId INT;
                declarationQuestionId INT;
            BEGIN
                
                SELECT id INTO formId FROM forms WHERE guid = '{formsGuid}';

                INSERT INTO form_sections (guid, title, form_id, type, display_order)
                VALUES ('{shareCodesSectionGuid}', 'Share my information consortium', formId, 1, 101);

                SELECT id INTO sectionId FROM form_sections WHERE guid = '{shareCodesSectionGuid}';

                INSERT INTO form_questions (guid, section_id, type, is_required, title, description, options, name)
                VALUES ('{Guid.NewGuid()}', sectionId, {(int)FormQuestionType.CheckYourAnswers}, TRUE, 'Check your answers', NULL, '{{}}', '_ShareMyInformationConsortium06')
                RETURNING id INTO previousQuestionId;

                INSERT INTO form_questions (guid, section_id, type, next_question_id, is_required, title, caption, description, options, summary_title, name)
                VALUES ('{Guid.NewGuid()}', sectionId, {(int)FormQuestionType.Address}, previousQuestionId, TRUE, 'Enter your postal address', 'Your declaration details', '<div id=""declarationNameHint"" class=""govuk-hint"">So the contracting authority can contact you.</div>', '{{""choices"": [{{""id"": ""bd4fe649-1f52-429e-978c-472e0a2cf11c"", ""title"": ""Address line 1"", ""groupName"": null, ""hint"": {{""title"": null, ""description"": ""Enter the first line of your address.""}}, ""value"": ""addressLine1""}}, {{""id"": ""a4edc9cd-f198-4e74-88e4-d981b09d30ed"", ""title"": ""Town or city"", ""groupName"": null, ""hint"": {{""title"": null, ""description"": ""Enter the name of your town or city.""}}, ""value"": ""townOrCity""}}, {{""id"": ""ef806205-6e38-4496-9a3e-8bb7a37b48ba"", ""title"": ""Post code"", ""groupName"": null, ""hint"": {{""title"": null, ""description"": ""Enter your postal code.""}}, ""value"": ""postCode""}}], ""choiceProviderStrategy"": null}}', 'Postal address', '_ShareMyInformationConsortium01')
                RETURNING id INTO previousQuestionId;

                INSERT INTO form_questions (guid, section_id, type, next_question_id, is_required, title, caption, description, options, summary_title, name)
                VALUES ('{Guid.NewGuid()}', sectionId, {(int)FormQuestionType.Text}, previousQuestionId, TRUE, 'Enter your email address', 'Your declaration details', '<div id=""declarationNameHint"" class=""govuk-hint"">So the contracting authority can contact you.</div>', '{{}}', 'Email address' , '_ShareMyInformationConsortium02')
                RETURNING id INTO previousQuestionId;

                INSERT INTO form_questions (guid, section_id, type, next_question_id, is_required, title, caption, description, options, summary_title, name)
                VALUES ('{Guid.NewGuid()}', sectionId, {(int)FormQuestionType.Text}, previousQuestionId, TRUE, 'Enter your job title', 'Your declaration details', NULL, '{{}}', 'Job title', '_ShareMyInformationConsortium03')
                RETURNING id INTO previousQuestionId;

                INSERT INTO form_questions (guid, section_id, type, next_question_id, is_required, title, caption, description, options, summary_title, name)
                VALUES ('{Guid.NewGuid()}', sectionId, {(int)FormQuestionType.Text}, previousQuestionId, TRUE, 'Enter your name', 'Your declaration details', '<div id=""declarationNameHint"" class=""govuk-hint"">Your name as the person authorised to declare the supplier information.</div>', '{{}}', 'Declared by', '_ShareMyInformationConsortium04')
                RETURNING id INTO previousQuestionId;

                INSERT INTO form_questions (guid, section_id, type, next_question_id, is_required, title, description, options, name)
                VALUES ('{Guid.NewGuid()}', sectionId, {(int)FormQuestionType.CheckBox}, previousQuestionId, TRUE, 'Declaration', '<ul class=""govuk-list govuk-list--bullet""><li>I am authorised to make this declaration on behalf of the supplier and declare that to the best of my knowledge the answers submitted and information contained is correct and accurate at the time of declaration.</li><br><li>I declare that, upon request from the Contracting Authority and without delay I will provide the certificates or documentary evidence referred to in this information.</li><br><li>I understand that the information is required as per the regulations of the Procurement Act 2023 and may be used in the selection process to assess my suitability to participate further in this procurement.</li><br><li>I understand that a contracting authority with whom this information is shared may request further clarity or detail on information provided in this submission.</li></ul>', '{{""choices"": [{{""id"": ""bd4fe649-1f52-429e-978c-472e0a2cf11c"", ""title"": ""I understand and agree to the above statements"", ""groupName"": null, ""hint"": null, ""value"": ""agree""}}], ""choiceProviderStrategy"": null}}', '_ShareMyInformationConsortium05')
                RETURNING id INTO declarationQuestionId;
            END $$;
        ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
            DO $$
            DECLARE
                sectionId INT;
            BEGIN
                SELECT id INTO sectionId FROM form_sections WHERE guid = '{shareCodesSectionGuid}';
                DELETE FROM form_questions WHERE section_id = sectionId;
                DELETE FROM form_sections WHERE id = sectionId;
                DELETE FROM forms WHERE guid = '{formsGuid}';
            END $$;");
           
        }
    }
}
