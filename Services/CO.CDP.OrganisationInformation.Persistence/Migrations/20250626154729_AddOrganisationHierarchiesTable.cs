using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganisationHierarchiesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "organisation_hierarchies",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    relationship_id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_organisation_id = table.Column<int>(type: "integer", nullable: false),
                    child_organisation_id = table.Column<int>(type: "integer", nullable: false),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    superseded_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organisation_hierarchies", x => x.id);
                    table.ForeignKey(
                        name: "fk_organisation_hierarchies_organisations_child_organisation_id",
                        column: x => x.child_organisation_id,
                        principalTable: "organisations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_organisation_hierarchies_organisations_parent_organisation_",
                        column: x => x.parent_organisation_id,
                        principalTable: "organisations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_organisation_hierarchies_child_organisation_id",
                table: "organisation_hierarchies",
                column: "child_organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_organisation_hierarchies_parent_organisation_id",
                table: "organisation_hierarchies",
                column: "parent_organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_organisation_hierarchies_parent_organisation_id_child_organ",
                table: "organisation_hierarchies",
                columns: new[] { "parent_organisation_id", "child_organisation_id", "superseded_on" },
                filter: "superseded_on IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_organisation_hierarchies_relationship_id",
                table: "organisation_hierarchies",
                column: "relationship_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "organisation_hierarchies");
        }
    }
}
