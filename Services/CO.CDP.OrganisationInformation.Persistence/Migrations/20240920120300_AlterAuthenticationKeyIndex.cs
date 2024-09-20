﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AlterAuthenticationKeyIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_authentication_keys_name_key",
                table: "authentication_keys");

            migrationBuilder.CreateIndex(
                name: "ix_authentication_keys_name_organisation_id",
                table: "authentication_keys",
                columns: new[] { "name", "organisation_id" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_authentication_keys_name_organisation_id",
                table: "authentication_keys");

            migrationBuilder.CreateIndex(
                name: "ix_authentication_keys_name_key",
                table: "authentication_keys",
                columns: new[] { "name", "key" },
                unique: true);
        }
    }
}
