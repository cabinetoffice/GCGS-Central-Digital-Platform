using CO.CDP.OrganisationInformation.Persistence.Forms;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TradeAssuranceForm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "display_order",
                table: "form_sections",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql($@"
                DO $$
                DECLARE
                    form_id int;
                    sectionId INT;
                    previousQuestionId INT;
                BEGIN
                    SELECT id INTO form_id FROM forms WHERE guid = '0618b13e-eaf2-46e3-a7d2-6f2c44be7022';

	                INSERT INTO form_sections (guid, title, form_id, allows_multiple_answer_sets, check_further_questions_exempted, type, configuration)
                    VALUES ('cf08acf8-e2fa-40c8-83e7-50c8671c343f', 'Trade assurances', form_id, TRUE, TRUE, 0, '{{""AddAnotherAnswerLabel"": ""Add another trade assurance?"", ""SingularSummaryHeading"": ""You have added 1 trade assurance"", ""RemoveConfirmationCaption"": ""Trade assurance"", ""RemoveConfirmationHeading"": ""Are you sure you want to remove this trade assurance?"", ""PluralSummaryHeadingFormat"": ""You have added {{0}} trade assurances"", ""FurtherQuestionsExemptedHeading"": ""<legend class=''govuk-fieldset__legend govuk-fieldset__legend--l''><h1 class=''govuk-fieldset__heading''>Do you want to add any trade assurances?</h1></legend><div class=''govuk-hint''>These are trade assurances for business and trade, not procurement specific. Adding them will provide confidence to contracting authorities and support any bids you submit. For example, Red Tractor Assurance covers food safety, traceability, animal welfare and environmental protection.</div>""}}');

                    SELECT id INTO sectionId FROM form_sections WHERE guid = 'cf08acf8-e2fa-40c8-83e7-50c8671c343f';

                    INSERT INTO form_questions (guid, section_id, type, is_required, title, description, options, name)
                    VALUES ('{Guid.NewGuid()}', sectionId, {(int)FormQuestionType.CheckYourAnswers}, TRUE, 'Check your answers', NULL, '{{}}', '_TradeAssurance04')
                    RETURNING id INTO previousQuestionId;

                    INSERT INTO form_questions (guid, section_id, type, next_question_id, is_required, title, description, options, caption, summary_title, name)
                    VALUES ('{Guid.NewGuid()}', sectionId, {(int)FormQuestionType.Date}, previousQuestionId, TRUE, 'What date was the trade assurance awarded?', '', '{{}}', NULL , 'Date awarded', '_TradeAssurance03')
                    RETURNING id INTO previousQuestionId;

                    INSERT INTO form_questions (guid, section_id, type, next_question_id, is_required, title, description, options, caption, summary_title, name)
                    VALUES ('{Guid.NewGuid()}', sectionId, {(int)FormQuestionType.Text}, previousQuestionId, FALSE, 'Do you know the reference number?', NULL, '{{}}', NULL, 'Reference number', '_TradeAssurance02')
                    RETURNING id INTO previousQuestionId;

                    INSERT INTO form_questions (guid, section_id, type, next_question_id, is_required, title, description, options, caption, summary_title, name)
                    VALUES ('{Guid.NewGuid()}', sectionId, {(int)FormQuestionType.Text}, previousQuestionId, TRUE, 'Who awarded the trade assurance?', '<div class=""govuk-hint"">Enter the name of the person or body. You can add another at the end if you need to. For example, Red Tractor Assurance, QMS Assurance.</div>', '{{}}', NULL, 'Awarded by', '_TradeAssurance01')
                    RETURNING id INTO previousQuestionId;
                END $$;
            ");

            migrationBuilder.Sql($@"
                update form_sections set display_order = 1 where title = 'Qualifications';
                update form_sections set display_order = 2 where title = 'Trade assurances';
                update form_sections set display_order = 3 where title = 'Exclusions';
                update form_sections set display_order = 4 where title = 'Financial Information';
                update form_sections set display_order = 100 where title = 'Share my information';
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
                    SELECT id INTO sectionId FROM form_sections WHERE guid = 'cf08acf8-e2fa-40c8-83e7-50c8671c343f';
                    DELETE FROM form_questions WHERE section_id = sectionId;
                    DELETE FROM form_sections WHERE id = sectionId;
                END $$;
            ");

            migrationBuilder.DropColumn(
                name: "display_order",
                table: "form_sections");
        }
    }
}
