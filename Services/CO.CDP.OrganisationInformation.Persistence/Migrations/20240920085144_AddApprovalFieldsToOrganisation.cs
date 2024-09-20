using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddApprovalFieldsToOrganisation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "approved_by_id",
                table: "organisations",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "approved_comment",
                table: "organisations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "approved_on",
                table: "organisations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_organisations_approved_by_id",
                table: "organisations",
                column: "approved_by_id");

            migrationBuilder.AddForeignKey(
                name: "fk_organisations_persons_approved_by_id",
                table: "organisations",
                column: "approved_by_id",
                principalTable: "persons",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_organisations_persons_approved_by_id",
                table: "organisations");

            migrationBuilder.DropIndex(
                name: "ix_organisations_approved_by_id",
                table: "organisations");

            migrationBuilder.DropColumn(
                name: "approved_by_id",
                table: "organisations");

            migrationBuilder.DropColumn(
                name: "approved_comment",
                table: "organisations");

            migrationBuilder.DropColumn(
                name: "approved_on",
                table: "organisations");
        }
    }
}
