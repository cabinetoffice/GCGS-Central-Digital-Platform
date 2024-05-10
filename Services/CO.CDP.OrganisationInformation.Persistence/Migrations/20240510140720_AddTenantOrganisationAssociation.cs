using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantOrganisationAssociation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Organisations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Organisations_TenantId",
                table: "Organisations",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Organisations_Tenants_TenantId",
                table: "Organisations",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Organisations_Tenants_TenantId",
                table: "Organisations");

            migrationBuilder.DropIndex(
                name: "IX_Organisations_TenantId",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Organisations");
        }
    }
}
