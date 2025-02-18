using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Add_Missing_Form_Translations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                DO $$
                BEGIN
                    UPDATE form_sections
                    SET title = 'Share_My_Information_Consortium_SectionTitle'
                    WHERE title = 'Share my information consortium';

                    UPDATE form_questions
                    SET title = 'Global_CheckYourAnswers'
                    WHERE title = 'Check your answers' and section_id = 5 and type = 6;

                    UPDATE form_questions
                    SET title = 'Global_Enter_Your_Postal_Address'
                    WHERE title = 'Enter your postal address' and section_id = 5 and type = 9;

                    UPDATE form_questions
                    SET title = 'Global_Enter_Your_Email_Address'
                    WHERE title = 'Enter your email address' and section_id = 5 and type = 1;

                    UPDATE form_questions
                    SET title = 'Global_Enter_Your_Job_Title'
                    WHERE title = 'Enter your job title' and section_id = 5 and type = 1;

                    UPDATE form_questions
                    SET title = 'Global_Enter_Your_Name'
                    WHERE title = 'Enter your name' and section_id = 5 and type = 1;

                    UPDATE form_questions
                    SET title = 'Global_CheckYourAnswers'
                    WHERE title = 'Check your answers' and section_id = 9 and type = 6;

                    UPDATE form_questions
                    SET title = 'Global_Enter_Your_Postal_Address'
                    WHERE title = 'Enter your postal address' and section_id = 9 and type = 9;

                    UPDATE form_questions
                    SET title = 'Global_Enter_Your_Email_Address'
                    WHERE title = 'Enter your email address' and section_id = 9 and type = 1;

                    UPDATE form_questions
                    SET title = 'Global_Enter_Your_Job_Title'
                    WHERE title = 'Enter your job title' and section_id = 9 and type = 1;

                    UPDATE form_questions
                    SET title = 'Global_Enter_Your_Name'
                    WHERE title = 'Enter your name' and section_id = 9 and type = 1;

                END $$;
             ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                DO $$
                BEGIN
                    UPDATE form_sections
                    SET title = 'Share my information consortium'
                    WHERE title = 'Share_My_Information_Consortium_SectionTitle';

                    UPDATE form_questions
                    SET title = 'Check your answers'
                    WHERE title = 'Global_CheckYourAnswers' and section_id = 5 and type = 6;

                    UPDATE form_questions
                    SET title = 'Enter your postal address'
                    WHERE title = 'Global_Enter_Your_Postal_Address' and section_id = 5 and type = 9;

                    UPDATE form_questions
                    SET title = 'Enter your email address'
                    WHERE title = 'Global_Enter_Your_Email_Address' and section_id = 5 and type = 1;

                    UPDATE form_questions
                    SET title = 'Enter your job title'
                    WHERE title = 'Global_Enter_Your_Job_Title' and section_id = 5 and type = 1;

                    UPDATE form_questions
                    SET title = 'Enter your name'
                    WHERE title = 'Global_Enter_Your_Name' and section_id = 5 and type = 1;

                    UPDATE form_questions
                    SET title = 'Check your answers'
                    WHERE title = 'Global_CheckYourAnswers' and section_id = 9 and type = 6;

                    UPDATE form_questions
                    SET title = 'Enter your postal address'
                    WHERE title = 'Global_Enter_Your_Postal_Address' and section_id = 9 and type = 9;

                    UPDATE form_questions
                    SET title = 'Enter your email address'
                    WHERE title = 'Global_Enter_Your_Email_Address' and section_id = 9 and type = 1;

                    UPDATE form_questions
                    SET title = 'Enter your job title'
                    WHERE title = 'Global_Enter_Your_Job_Title' and section_id = 9 and type = 1;

                    UPDATE form_questions
                    SET title = 'Enter your name'
                    WHERE title = 'Global_Enter_Your_Name' and section_id = 9 and type = 1;

                END $$;
             ");
        }
    }
}
