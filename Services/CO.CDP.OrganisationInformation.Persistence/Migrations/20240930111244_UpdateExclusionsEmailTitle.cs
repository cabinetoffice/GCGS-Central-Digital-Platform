using Microsoft.EntityFrameworkCore.Migrations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Collections.Generic;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateExclusionsEmailTitle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                DO $$
                BEGIN
                    UPDATE form_questions
                    SET title = 'Enter an email address'
                    WHERE name = '_Exclusion06';
                END $$;
             ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                DO $$
                BEGIN
                    UPDATE form_questions
                    SET title = 'Enter the email address?'
                    WHERE name = '_Exclusion06';
                END $$;
             ");
        }
    }
}
