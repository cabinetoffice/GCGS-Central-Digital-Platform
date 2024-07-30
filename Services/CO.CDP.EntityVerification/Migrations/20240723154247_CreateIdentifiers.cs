using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CO.CDP.EntityVerification.Migrations
{
    /// <inheritdoc />
    public partial class CreateIdentifiers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_ppon",
                schema: "entity_verification",
                table: "ppon");

            migrationBuilder.DropIndex(
                name: "ix_ppon_ppon_id",
                schema: "entity_verification",
                table: "ppon");

            migrationBuilder.RenameTable(
                name: "ppon",
                schema: "entity_verification",
                newName: "ppons",
                newSchema: "entity_verification");

            migrationBuilder.RenameColumn(
                name: "ppon_id",
                schema: "entity_verification",
                table: "ppons",
                newName: "identifier_id");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_on",
                schema: "entity_verification",
                table: "ppons",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<string>(
                name: "name",
                schema: "entity_verification",
                table: "ppons",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "organisation_id",
                schema: "entity_verification",
                table: "ppons",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "updated_on",
                schema: "entity_verification",
                table: "ppons",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddPrimaryKey(
                name: "pk_ppons",
                schema: "entity_verification",
                table: "ppons",
                column: "id");

            migrationBuilder.CreateTable(
                name: "identifiers",
                schema: "entity_verification",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    identifier_id = table.Column<string>(type: "text", nullable: false),
                    scheme = table.Column<string>(type: "text", nullable: false),
                    legal_name = table.Column<string>(type: "text", nullable: false),
                    uri = table.Column<string>(type: "text", nullable: true),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ppon_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identifiers", x => x.id);
                    table.ForeignKey(
                        name: "fk_identifiers_ppons_ppon_id",
                        column: x => x.ppon_id,
                        principalSchema: "entity_verification",
                        principalTable: "ppons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_ppons_identifier_id",
                schema: "entity_verification",
                table: "ppons",
                column: "identifier_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_ppons_name",
                schema: "entity_verification",
                table: "ppons",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "ix_ppons_organisation_id",
                schema: "entity_verification",
                table: "ppons",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_identifiers_identifier_id",
                schema: "entity_verification",
                table: "identifiers",
                column: "identifier_id");

            migrationBuilder.CreateIndex(
                name: "ix_identifiers_ppon_id",
                schema: "entity_verification",
                table: "identifiers",
                column: "ppon_id");

            migrationBuilder.CreateIndex(
                name: "ix_identifiers_scheme",
                schema: "entity_verification",
                table: "identifiers",
                column: "scheme");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "identifiers",
                schema: "entity_verification");

            migrationBuilder.DropPrimaryKey(
                name: "pk_ppons",
                schema: "entity_verification",
                table: "ppons");

            migrationBuilder.DropIndex(
                name: "ix_ppons_identifier_id",
                schema: "entity_verification",
                table: "ppons");

            migrationBuilder.DropIndex(
                name: "ix_ppons_name",
                schema: "entity_verification",
                table: "ppons");

            migrationBuilder.DropIndex(
                name: "ix_ppons_organisation_id",
                schema: "entity_verification",
                table: "ppons");

            migrationBuilder.DropColumn(
                name: "created_on",
                schema: "entity_verification",
                table: "ppons");

            migrationBuilder.DropColumn(
                name: "name",
                schema: "entity_verification",
                table: "ppons");

            migrationBuilder.DropColumn(
                name: "organisation_id",
                schema: "entity_verification",
                table: "ppons");

            migrationBuilder.DropColumn(
                name: "updated_on",
                schema: "entity_verification",
                table: "ppons");

            migrationBuilder.RenameTable(
                name: "ppons",
                schema: "entity_verification",
                newName: "ppon",
                newSchema: "entity_verification");

            migrationBuilder.RenameColumn(
                name: "identifier_id",
                schema: "entity_verification",
                table: "ppon",
                newName: "ppon_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_ppon",
                schema: "entity_verification",
                table: "ppon",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_ppon_ppon_id",
                schema: "entity_verification",
                table: "ppon",
                column: "ppon_id",
                unique: true);
        }
    }
}
