using CO.CDP.OrganisationInformation.Persistence.Forms;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExclusionSectionUpload : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE form_questions SET is_required = FALSE, caption = '<legend class=""govuk-fieldset__legend"">Enter the date the circumstances ended</legend><div class=""govuk-hint"">For example, 27 3 2007</div>' WHERE name = '_Exclusion02';

                UPDATE form_questions SET is_required = FALSE WHERE name = '_Exclusion03';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE form_questions SET is_required = TRUE, caption = 'Enter the date the circumstances ended, For example, 05 04 2022' WHERE name = '_Exclusion02';

                UPDATE form_questions SET is_required = TRUE WHERE name = '_Exclusion03';
            ");
        }
    }
}