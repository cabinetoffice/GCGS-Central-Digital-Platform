using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Outbox_AddColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "queue_url",
                table: "outbox_messages",
                type: "text",
                defaultValue: "",
                nullable: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "message_group_id",
                table: "outbox_messages",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "queue_url",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "message_group_id",
                table: "outbox_messages");
        }
    }
}
