using CO.CDP.OrganisationInformation.Persistence.Forms;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExclusionFormData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                DO $$
                DECLARE
                    sectionId INT;
                    previousQuestionId INT;
                BEGIN
                    SELECT id INTO sectionId FROM form_sections WHERE guid = '8a75cb04-fe29-45ae-90f9-168832dbea48';

                    INSERT INTO form_questions (guid, section_id, type, is_required, title, description, options, name)
                    VALUES ('{Guid.NewGuid()}', sectionId, {(int)FormQuestionType.CheckYourAnswers}, TRUE, 'Check your answers', NULL, '{{}}', '_Exclusion01')
                    RETURNING id INTO previousQuestionId;

                    INSERT INTO form_questions (guid, section_id, type, next_question_id, is_required, title, description, options, caption, summary_title, name)
                    VALUES ('{Guid.NewGuid()}', sectionId, {(int)FormQuestionType.Date}, previousQuestionId, TRUE, 'Have the circumstances that led to the exclusion ended?', '<div id=""isEventEnded-hint"" class=""govuk-hint"">For example, a court decision for environmental misconduct led your organisation or the connected person to stop harming the environment.</div>', '{{}}', 'Enter the date the circumstances ended, For example, 05 04 2022' , 'Date circumstances ended', '_Exclusion02')
                    RETURNING id INTO previousQuestionId;

                    INSERT INTO form_questions (guid, section_id, type, next_question_id, is_required, title, description, options, caption, summary_title, name)
                    VALUES ('{Guid.NewGuid()}', sectionId, {(int)FormQuestionType.FileUpload}, previousQuestionId, TRUE, 'Do you have a supporting document to upload?', '<div id=""documents-hint"" class=""govuk-hint"">A decision from a public authority that was the basis for the offence. For example, documentation from the police, HMRC or the court.</div>', '{{}}', 'Upload a file, You can upload most file types including: PDF, scans, mobile phone photos, Word, Excel and PowerPoint, multimedia and ZIP files that are smaller than 25MB.', 'Supporting document', '_Exclusion03')
                    RETURNING id INTO previousQuestionId;

                    INSERT INTO form_questions (guid, section_id, type, next_question_id, is_required, title, description, options, caption, summary_title, name)
                    VALUES ('{Guid.NewGuid()}', sectionId, {(int)FormQuestionType.MultiLine}, previousQuestionId, TRUE, 'How the exclusion is being managed', '<ul class=""govuk-list govuk-list--bullet""><li>have done to prove it was taken seriously - for example, paid a fine or compensation</li><li>have done to stop the circumstances that caused it from happening again - for example, taking steps like changing staff or management or putting procedures or training in place</li><li>are doing to monitor the steps that were taken - for example, regular meetings</li></ul>', '{{}}','You must tell us what you or the person who was subject to the event:' , 'Exclusion being managed', '_Exclusion04')
                    RETURNING id INTO previousQuestionId;

                    INSERT INTO form_questions (guid, section_id, type, next_question_id, is_required, title, description, options, caption, summary_title, name)
                    VALUES ('{Guid.NewGuid()}', sectionId, {(int)FormQuestionType.MultiLine}, previousQuestionId, TRUE, 'Describe the exclusion in more detail', NULL, '{{}}', 'Give us your explanation of the event. For example, any background information you can give about what happened or what caused the exclusion.', 'Exclusion in detail', '_Exclusion05')
                    RETURNING id INTO previousQuestionId;

                    INSERT INTO form_questions (guid, section_id, type, next_question_id, is_required, title, description, options, caption, summary_title, name)
                    VALUES ('{Guid.NewGuid()}', sectionId, {(int)FormQuestionType.Text}, previousQuestionId, TRUE, 'Enter the email address?', NULL, '{{}}', 'Where the contracting authority can contact someone about the exclusion', 'Contact email', '_Exclusion06')
                    RETURNING id INTO previousQuestionId;

                    INSERT INTO form_questions (guid, section_id, type, next_question_id, is_required, title, description, options, caption, summary_title, name)
                    VALUES ('{Guid.NewGuid()}', sectionId, {(int)FormQuestionType.YesOrNo}, previousQuestionId, TRUE, 'Did this exclusion happen in the UK?', NULL, '{{}}', NULL, 'UK exclusion', '_Exclusion07')
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
                    SELECT id INTO sectionId FROM form_sections WHERE guid = '8a75cb04-fe29-45ae-90f9-168832dbea48';
 
                    DELETE FROM form_questions WHERE section_id = sectionId;
                END $$;
            ");
        }
    }
}
