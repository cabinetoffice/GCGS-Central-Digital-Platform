using CO.CDP.OrganisationInformation.Persistence.Forms;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BasicInformationQualifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                DO $$
                DECLARE
                    form_id int;
                    sectionId INT;
                    previousQuestionId INT;
                BEGIN
                    SELECT id INTO form_id FROM forms WHERE guid = '0618b13e-eaf2-46e3-a7d2-6f2c44be7022';

	                INSERT INTO form_sections (guid, title, form_id, allows_multiple_answer_sets, type, configuration)
                    VALUES ('798cf1c1-40be-4e49-9adb-252219d5599d', 'Qualifications', form_id, TRUE, 0, '{{""AddAnotherAnswerLabel"": ""Add another qualification?"", ""SingularSummaryHeading"": ""You have added 1 qualification"", ""RemoveConfirmationCaption"": ""Qualifications"", ""RemoveConfirmationHeading"": ""Are you sure you want to remove this qualification?"", ""PluralSummaryHeadingFormat"": ""You have added {{0}} qualifications""}}');

                    SELECT id INTO sectionId FROM form_sections WHERE guid = '798cf1c1-40be-4e49-9adb-252219d5599d';

                    INSERT INTO form_questions (guid, section_id, type, is_required, title, description, options, name)
                    VALUES ('{Guid.NewGuid()}', sectionId, {(int)FormQuestionType.CheckYourAnswers}, TRUE, 'Check your answers', NULL, '{{}}', '_Qualifications04')
                    RETURNING id INTO previousQuestionId;

                    INSERT INTO form_questions (guid, section_id, type, next_question_id, is_required, title, description, options, caption, summary_title, name)
                    VALUES ('{Guid.NewGuid()}', sectionId, {(int)FormQuestionType.Date}, previousQuestionId, TRUE, 'What date was the qualification awarded?', '', '{{}}', NULL , 'Date awarded', '_Qualifications03')
                    RETURNING id INTO previousQuestionId;

                    INSERT INTO form_questions (guid, section_id, type, next_question_id, is_required, title, description, options, caption, summary_title, name)
                    VALUES ('{Guid.NewGuid()}', sectionId, {(int)FormQuestionType.Text}, previousQuestionId, TRUE, 'Who awarded the qualification?', '<div class=""govuk-hint"">Enter the name of the person or body. For example, ISO, Constructionline or Red Tractor Assurance.</div>', '{{}}', NULL, 'Awarded by', '_Qualifications02')
                    RETURNING id INTO previousQuestionId;

                    INSERT INTO form_questions (guid, section_id, type, next_question_id, is_required, title, description, options, caption, summary_title, name)
                    VALUES ('{Guid.NewGuid()}', sectionId, {(int)FormQuestionType.Text}, previousQuestionId, TRUE, 'Enter the qualification name', '<div class=""govuk-hint"">Enter one qualification at a time. You can add another at the end if you need to. For example, ISO 45001 Health and Safety Management.</div>', '{{}}', NULL, 'Qualification name', '_Qualifications01')
                    RETURNING id INTO previousQuestionId;
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
                    SELECT id INTO sectionId FROM form_sections WHERE guid = '798cf1c1-40be-4e49-9adb-252219d5599d';
 
                    DELETE FROM form_questions WHERE section_id = sectionId;

                    DELETE FROM form_sections WHERE guid = sectionId;
                END $$;
            ");
        }
    }
}
