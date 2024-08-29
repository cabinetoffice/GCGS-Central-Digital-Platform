using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FormAddressAnswerDataUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"UPDATE form_answers 
                SET address_value = address_value || '{""Country"":""GB""}'
                WHERE address_value ->> 'CountryName' = 'United Kingdom';

                UPDATE form_answers 
                SET address_value = address_value || '{""Country"":""""}'
                WHERE address_value ->> 'CountryName' != 'United Kingdom';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"UPDATE form_answers SET address_value = address_value - 'Country' where address_value->> 'CountryName' != '';");
        }
    }
}