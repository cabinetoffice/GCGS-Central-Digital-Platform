using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CO.CDP.Organisation.Persistence.Migrations;

/// <inheritdoc />
public partial class CreateOrganisation : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Organisations",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Guid = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                Identifier_Id = table.Column<string>(type: "text", nullable: false),
                Identifier_LegalName = table.Column<string>(type: "text", nullable: false),
                Identifier_Scheme = table.Column<string>(type: "text", nullable: false),
                Identifier_Uri = table.Column<string>(type: "text", nullable: false),
                Roles = table.Column<List<int>>(type: "integer[]", nullable: false),
                Address_CountryName = table.Column<string>(type: "text", nullable: false),
                Address_Locality = table.Column<string>(type: "text", nullable: false),
                Address_PostalCode = table.Column<string>(type: "text", nullable: false),
                Address_Region = table.Column<string>(type: "text", nullable: false),
                Address_StreetAddress = table.Column<string>(type: "text", nullable: false),
                ContactPoint_Email = table.Column<string>(type: "text", nullable: false),
                ContactPoint_FaxNumber = table.Column<string>(type: "text", nullable: false),
                ContactPoint_Name = table.Column<string>(type: "text", nullable: false),
                ContactPoint_Telephone = table.Column<string>(type: "text", nullable: false),
                ContactPoint_Url = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Organisations", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Organisations_AdditionalIdentifiers",
            columns: table => new
            {
                OrganisationId = table.Column<int>(type: "integer", nullable: false),
                Id = table.Column<string>(type: "text", nullable: false),
                LegalName = table.Column<string>(type: "text", nullable: false),
                Scheme = table.Column<string>(type: "text", nullable: false),
                Uri = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Organisations_AdditionalIdentifiers", x => new { x.OrganisationId, x.Id });
                table.ForeignKey(
                    name: "FK_Organisations_AdditionalIdentifiers_Organisations_Organisat~",
                    column: x => x.OrganisationId,
                    principalTable: "Organisations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Organisations_Guid",
            table: "Organisations",
            column: "Guid",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Organisations_Name",
            table: "Organisations",
            column: "Name",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Organisations_AdditionalIdentifiers");

        migrationBuilder.DropTable(
            name: "Organisations");
    }
}
