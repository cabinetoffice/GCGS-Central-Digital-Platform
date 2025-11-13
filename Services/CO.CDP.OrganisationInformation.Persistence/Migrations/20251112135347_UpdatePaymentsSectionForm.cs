using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePaymentsSectionForm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$
                BEGIN

                    IF EXISTS (SELECT 1 FROM form_questions WHERE title = 'Payments_01_Title') THEN
                        UPDATE form_questions
                        SET ""options"" = '{""layout"": {""button"": {""style"": ""Start"", ""text"": ""Global_Start""}}}'::jsonb
                        WHERE title = 'Payments_01_Title';
                    END IF;

                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$
                BEGIN

                    IF EXISTS (SELECT 1 FROM form_questions WHERE title = 'Payments_01_Title') THEN
                        UPDATE form_questions
                        SET ""options"" = '{}'::jsonb
                        WHERE title = 'Payments_01_Title';
                    END IF;

                END $$;
            ");
        }
    }
}