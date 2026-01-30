using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePaymentsQuestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = $@"
                DO $$
                BEGIN
                    -- update alternate question navigation for question 09 to go to question 11
                    UPDATE form_questions SET next_question_alternative_id = (SELECT id FROM form_questions WHERE title = 'Payments_11_Title') WHERE title = 'Payments_09_Title';

                    -- update next question navigation for question 11 to go to question 10
                    UPDATE form_questions SET next_question_id = (SELECT id FROM form_questions WHERE title = 'Payments_10_Title') WHERE title = 'Payments_11_Title';

                    -- update alternate question navigation for question 10 to NULL as no longer required to branch
                    UPDATE form_questions SET next_question_alternative_id = NULL WHERE title = 'Payments_10_Title';

                END $$;
            ";

            migrationBuilder.Sql(sql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var sql = $@"
                DO $$
                DECLARE
                    section_id_val INT;
                    cya_id INT;
                BEGIN
                    SELECT id INTO section_id_val FROM form_sections WHERE title = 'Payments_SectionTitle';
                    SELECT id INTO cya_id FROM form_questions WHERE title = 'Global_CheckYourAnswers' AND section_id = section_id_val;

                    -- revert alternate question navigation for question 09
                    UPDATE form_questions SET next_question_alternative_id = NULL WHERE title = 'Payments_09_Title';

                    -- revert next question navigation for question 11 to go to check your answers
                    UPDATE form_questions SET next_question_id = cya_id WHERE title = 'Payments_11_Title';

                    -- revert alternate question navigation for question 10 to go to question 11
                    UPDATE form_questions SET next_question_alternative_id = (SELECT id FROM form_questions WHERE title = 'Payments_11_Title') WHERE title = 'Payments_10_Title';

                END $$;
            ";

            migrationBuilder.Sql(sql);
        }
    }
}
