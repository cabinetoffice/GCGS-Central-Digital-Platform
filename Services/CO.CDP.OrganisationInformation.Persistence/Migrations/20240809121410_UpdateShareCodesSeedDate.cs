using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateShareCodesSeedDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "summary_title",
                table: "form_questions",
                type: "text",
                nullable: true);

            migrationBuilder.Sql($@"
                UPDATE form_questions
                SET summary_title = 'Audited accounts'
                WHERE title = 'Were your accounts audited?';

                UPDATE form_questions
                SET summary_title = 'Document uploaded'
                WHERE title = 'Upload your accounts';

                UPDATE form_questions
                SET summary_title = 'Date of financial year end'
                WHERE title = 'What is the financial year end date for the information you uploaded?';

                UPDATE form_questions
                SET summary_title = 'Declared by'
                WHERE title = 'Enter your name';

                UPDATE form_questions
                SET summary_title = 'Job title'
                WHERE title = 'Enter your job title';

                UPDATE form_questions
                SET summary_title = 'Email address'
                WHERE title = 'Enter your email address';

                UPDATE form_questions
                SET summary_title = 'Postal address'
                WHERE title = 'Enter your postal address';
            ");

            migrationBuilder.Sql($@"
                UPDATE form_questions 
                SET caption = 'Your declaration details', 
                description = '<div id=""declarationNameEmailHint"" class=""govuk-hint"">Your name as the person authorised to declare the supplier information.</div>'
                WHERE title = 'Enter your name';

                UPDATE form_questions 
                SET caption = 'Your declaration details', 
                description = null
                WHERE title = 'Enter your job title';

                UPDATE form_questions 
                SET caption = 'Your declaration details', 
                description = '<div id=""declarationNameEmailHint"" class=""govuk-hint"">So the contracting authority can contact you.</div>'
                WHERE title = 'Enter your email address';

                UPDATE form_questions
                SET caption = 'Your declaration details', 
                description = '<div id=""declarationNameEmailHint"" class=""govuk-hint"">So the contracting authority can contact you.</div>'
                WHERE title = 'Enter your postal address';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "summary_title",
                table: "form_questions");

            migrationBuilder.Sql($@"
                UPDATE form_questions
                SET caption = '<div id=""declarationNameEmailHint"" class=""govuk-hint"">Your name as the person authorised to declare the supplier information.</div>', 
                description = 'Your declaration details'
                WHERE title = 'Enter your name';

                UPDATE form_questions
                SET caption = null, 
                description = 'Your declaration details'
                WHERE title = 'Enter your job title';

                UPDATE form_questions
                SET caption = '<div id=""declarationNameEmailHint"" class=""govuk-hint"">So the contracting authority can contact you.</div>', 
                description = 'Your declaration details'
                WHERE title = 'Enter your email address';

                UPDATE form_questions
                SET caption = '<div id=""declarationNameEmailHint"" class=""govuk-hint"">So the contracting authority can contact you.</div>', 
                description = 'Your declaration details'
                WHERE title = 'Enter your postal address';
            ");
        }
    }
}
