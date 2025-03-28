using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDescriptionHintFinancialInformation03 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                UPDATE
                    form_questions
                SET
                    description = '<p class=""govuk-body"">Auditing requirements are defined in <a href=""https://www.legislation.gov.uk/ukpga/2006/46/part/16"" target=""https://www.legislation.gov.uk/ukpga/2006/46/part/16"">part 16 of the Companies Act 2006 (opens in new tab)</a> or an overseas equivalent</p>'                
                WHERE
                    name = '_FinancialInformation03'
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
