using CO.CDP.OrganisationInformation.Persistence.Forms;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExclusionAppliesToQuestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                DO $$
                DECLARE
                    sectionId INT;
                    questionId INT;
                BEGIN
                    SELECT id INTO sectionId FROM form_sections WHERE guid = '8a75cb04-fe29-45ae-90f9-168832dbea48';
 
                    SELECT id INTO questionId FROM form_questions WHERE name = '_Exclusion06';
 
                    INSERT INTO form_questions (guid, section_id, next_question_id, type, is_required, title, description, options, name, summary_title)
                    VALUES ('{Guid.NewGuid()}',
                        sectionId,
                        questionId,
                        {(int)FormQuestionType.SingleChoice},
                        TRUE,
                        'Select who the exclusion applies to',
                        '<div class=""govuk-inset-text govuk-!-margin-top-0"">If it applies to someone not listed, you must go back to the ‘Add a connected person’ section and add them.</div>',
                        '{{ ""choiceProviderStrategy"": ""ExclusionAppliesToChoiceProviderStrategy"", ""choices"": []}}',
                        '_Exclusion09',
                        'Exclusion applies to')
                    RETURNING id INTO questionId;
 
                    UPDATE form_questions SET next_question_id = questionId WHERE name = '_Exclusion08';
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                DO $$
                DECLARE
                    questionId INT;
                BEGIN 
                    SELECT id INTO questionId FROM form_questions WHERE name = '_Exclusion06';
 
                    UPDATE form_questions SET next_question_id = questionId WHERE name = '_Exclusion08';
 
                    DELETE FROM form_questions WHERE name = '_Exclusion09';
                END $$;
            ");
        }
    }
}
