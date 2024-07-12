using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CO.CDP.EntityVerification.Migrations
{
    /// <inheritdoc />
    public partial class CreateEVSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "entity_validation");

            migrationBuilder.CreateTable(
                name: "ppon",
                schema: "entity_validation",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ppon_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ppon", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_ppon_ppon_id",
                schema: "entity_validation",
                table: "ppon",
                column: "ppon_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ppon",
                schema: "entity_validation");
        }
    }
}
