using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class OrganisationRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address_CountryName",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "Address_Locality",
                table: "Organisations");

            migrationBuilder.RenameColumn(
                name: "Roles",
                table: "Organisations",
                newName: "Types");

            migrationBuilder.RenameColumn(
                name: "ContactPoint_FaxNumber",
                table: "Organisations",
                newName: "Identifier_Number");

            migrationBuilder.RenameColumn(
                name: "Address_StreetAddress",
                table: "Organisations",
                newName: "Address_PostCode");

            migrationBuilder.RenameColumn(
                name: "Address_Region",
                table: "Organisations",
                newName: "Address_City");

            migrationBuilder.RenameColumn(
                name: "Address_PostalCode",
                table: "Organisations",
                newName: "Address_AddressLine1");

            migrationBuilder.AlterColumn<string>(
                name: "Uri",
                table: "Organisations_AdditionalIdentifiers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "LegalName",
                table: "Organisations_AdditionalIdentifiers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Number",
                table: "Organisations_AdditionalIdentifiers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Identifier_Uri",
                table: "Organisations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Identifier_LegalName",
                table: "Organisations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Identifier_Id",
                table: "Organisations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ContactPoint_Url",
                table: "Organisations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ContactPoint_Telephone",
                table: "Organisations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ContactPoint_Name",
                table: "Organisations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Number",
                table: "Organisations_AdditionalIdentifiers");

            migrationBuilder.DropColumn(
                name: "Address_AddressLine2",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "Address_Country",
                table: "Organisations");

            migrationBuilder.RenameColumn(
                name: "Types",
                table: "Organisations",
                newName: "Roles");

            migrationBuilder.RenameColumn(
                name: "Identifier_Number",
                table: "Organisations",
                newName: "ContactPoint_FaxNumber");

            migrationBuilder.RenameColumn(
                name: "Address_PostCode",
                table: "Organisations",
                newName: "Address_StreetAddress");

            migrationBuilder.RenameColumn(
                name: "Address_City",
                table: "Organisations",
                newName: "Address_Region");

            migrationBuilder.RenameColumn(
                name: "Address_AddressLine1",
                table: "Organisations",
                newName: "Address_PostalCode");

            migrationBuilder.AlterColumn<string>(
                name: "Uri",
                table: "Organisations_AdditionalIdentifiers",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LegalName",
                table: "Organisations_AdditionalIdentifiers",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Identifier_Uri",
                table: "Organisations",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Identifier_LegalName",
                table: "Organisations",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Identifier_Id",
                table: "Organisations",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ContactPoint_Url",
                table: "Organisations",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ContactPoint_Telephone",
                table: "Organisations",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ContactPoint_Name",
                table: "Organisations",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

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
    }
}
