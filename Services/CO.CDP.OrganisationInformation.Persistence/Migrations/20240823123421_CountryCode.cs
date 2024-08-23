using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CountryCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "country",
                table: "addresses",
                type: "text",
                nullable: false,
                defaultValue: "");

            // set all country code to GB, this is okay for now as we havn't got to prod yet
            migrationBuilder.Sql("UPDATE addresses SET country = 'GB', country_name = 'United Kingdom';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "country",
                table: "addresses");
        }
    }
}
