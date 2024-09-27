using CO.CDP.OrganisationInformation.Persistence.Forms;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExclusionWebsiteQuestion : Migration
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
 
                    SELECT id INTO questionId FROM form_questions WHERE name = '_Exclusion02';
 
                    INSERT INTO form_questions (guid, section_id, next_question_id, type, is_required, title, description, options, name, summary_title)
                    VALUES ('{Guid.NewGuid()}',
                        sectionId,
                        questionId,
                        {(int)FormQuestionType.Url},
                        FALSE,
                        'Was the decision recorded on a public authority website?',
                        '<div class=""govuk-hint"">For example, the outcome of a court decision for a conviction or other event</div>',
                        '{{}}',
                        '_Exclusion10',
                        'Recorded on a website')
                    RETURNING id INTO questionId;
 
                    UPDATE form_questions SET next_question_id = questionId WHERE name = '_Exclusion03';
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
                    SELECT id INTO questionId FROM form_questions WHERE name = '_Exclusion02';
 
                    UPDATE form_questions SET next_question_id = questionId WHERE name = '_Exclusion03';
 
                    DELETE FROM form_questions WHERE name = '_Exclusion10';
                END $$;
            ");
        }
    }
}
