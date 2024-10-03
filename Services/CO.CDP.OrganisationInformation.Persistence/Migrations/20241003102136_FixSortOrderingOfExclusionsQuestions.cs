using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixSortOrderingOfExclusionsQuestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                DO $$
                DECLARE
                    questionId INT;
                BEGIN
                    SELECT id INTO questionId FROM form_questions WHERE name = '_Exclusion08'; 
                    UPDATE form_questions SET next_question_id = questionId WHERE name = '_Exclusion07';

                    SELECT id INTO questionId FROM form_questions WHERE name = '_Exclusion09'; 
                    UPDATE form_questions SET next_question_id = questionId WHERE name = '_Exclusion08';

                    SELECT id INTO questionId FROM form_questions WHERE name = '_Exclusion06'; 
                    UPDATE form_questions SET next_question_id = questionId WHERE name = '_Exclusion09';

                    SELECT id INTO questionId FROM form_questions WHERE name = '_Exclusion05'; 
                    UPDATE form_questions SET next_question_id = questionId WHERE name = '_Exclusion06';

                    SELECT id INTO questionId FROM form_questions WHERE name = '_Exclusion04'; 
                    UPDATE form_questions SET next_question_id = questionId WHERE name = '_Exclusion05';

                    SELECT id INTO questionId FROM form_questions WHERE name = '_Exclusion03'; 
                    UPDATE form_questions SET next_question_id = questionId WHERE name = '_Exclusion04';

                    SELECT id INTO questionId FROM form_questions WHERE name = '_Exclusion10'; 
                    UPDATE form_questions SET next_question_id = questionId WHERE name = '_Exclusion03';

                    SELECT id INTO questionId FROM form_questions WHERE name = '_Exclusion02'; 
                    UPDATE form_questions SET next_question_id = questionId WHERE name = '_Exclusion10';

                    SELECT id INTO questionId FROM form_questions WHERE name = '_Exclusion01'; 
                    UPDATE form_questions SET next_question_id = questionId WHERE name = '_Exclusion02';
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
