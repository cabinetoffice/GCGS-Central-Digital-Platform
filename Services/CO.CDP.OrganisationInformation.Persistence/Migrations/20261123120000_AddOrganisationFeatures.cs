using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <summary>
    /// Adds the organisation_features table, which stores per-organisation feature access
    /// (for example "ai-tool"). Rows are granted manually with SQL; there is no API to edit them.
    /// </summary>
    [DbContext(typeof(OrganisationInformationContext))]
    [Migration("20261123120000_AddOrganisationFeatures")]
    public class AddOrganisationFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "organisation_features",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    organisation_id = table.Column<int>(type: "integer", nullable: false),
                    feature = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organisation_features", x => x.id);
                    table.ForeignKey(
                        name: "fk_organisation_features_organisations_organisation_id",
                        column: x => x.organisation_id,
                        principalTable: "organisations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_organisation_features_organisation_id_feature",
                table: "organisation_features",
                columns: new[] { "organisation_id", "feature" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "organisation_features");
        }
    }
}
