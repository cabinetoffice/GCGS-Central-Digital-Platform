using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.EntityVerification.Migrations
{
    /// <inheritdoc />
    public partial class Outbox_AddColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.AddColumn<DateTimeOffset>(
                name: "queue_url",
                schema: "entity_verification",
                table: "outbox_messages",
                type: "text",
                defaultValue: "",
                nullable: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "message_group_id",
                schema: "entity_verification",
                table: "outbox_messages",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "queue_url",
                schema: "entity_verification",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "message_group_id",
                schema: "entity_verification",
                table: "outbox_messages");
        }
    }
}
