using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFutureDateValidation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM form_questions WHERE title = 'CyberEssentials_05_Title') THEN
                        UPDATE form_questions
                        SET options = jsonb_set(
                            options::jsonb,
                            '{validation}',
                            '{""dateValidationType"": ""FutureOnly""}'::jsonb,
                            true
                        )
                        WHERE title = 'CyberEssentials_05_Title';
                    END IF;

                    IF EXISTS (SELECT 1 FROM form_questions WHERE title = 'CyberEssentials_09_Title') THEN
                        UPDATE form_questions
                        SET options = jsonb_set(
                            options::jsonb,
                            '{validation}',
                            '{""dateValidationType"": ""FutureOnly""}'::jsonb,
                            true
                        )
                        WHERE title = 'CyberEssentials_09_Title';
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
                    IF EXISTS (SELECT 1 FROM form_questions WHERE title = 'CyberEssentials_05_Title') THEN
                        UPDATE form_questions
                        SET options = options::jsonb - 'validation'
                        WHERE title = 'CyberEssentials_05_Title';
                    END IF;

                    IF EXISTS (SELECT 1 FROM form_questions WHERE title = 'CyberEssentials_09_Title') THEN
                        UPDATE form_questions
                        SET options = options::jsonb - 'validation'
                        WHERE title = 'CyberEssentials_09_Title';
                    END IF;
                END $$;
            ");
        }
    }
}
