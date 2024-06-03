using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameAddressFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address_AddressLine2",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "Address_Country",
                table: "Organisations");

            migrationBuilder.RenameColumn(
                name: "Address_PostCode",
                table: "Organisations",
                newName: "Address_StreetAddress2");

            migrationBuilder.RenameColumn(
                name: "Address_City",
                table: "Organisations",
                newName: "Address_StreetAddress");

            migrationBuilder.RenameColumn(
                name: "Address_AddressLine1",
                table: "Organisations",
                newName: "Address_PostalCode");

            migrationBuilder.AddColumn<string>(
                name: "Address_CountryName",
                table: "Organisations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Address_Locality",
                table: "Organisations",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address_CountryName",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "Address_Locality",
                table: "Organisations");

            migrationBuilder.RenameColumn(
                name: "Address_StreetAddress2",
                table: "Organisations",
                newName: "Address_PostCode");

            migrationBuilder.RenameColumn(
                name: "Address_StreetAddress",
                table: "Organisations",
                newName: "Address_City");

            migrationBuilder.RenameColumn(
                name: "Address_PostalCode",
                table: "Organisations",
                newName: "Address_AddressLine1");

            migrationBuilder.AddColumn<string>(
                name: "Address_AddressLine2",
                table: "Organisations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_Country",
                table: "Organisations",
                type: "text",
                nullable: true);
        }
    }
}