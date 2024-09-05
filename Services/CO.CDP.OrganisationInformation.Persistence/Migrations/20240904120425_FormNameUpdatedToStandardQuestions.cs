using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FormNameUpdatedToStandardQuestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var financialInformationFormsGuid = Guid.Parse("0618b13e-eaf2-46e3-a7d2-6f2c44be7022");

            migrationBuilder.Sql($@"
                UPDATE forms
                SET name = 'Standard Questions'
                WHERE guid ='{financialInformationFormsGuid}';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
