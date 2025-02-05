using Microsoft.EntityFrameworkCore.Migrations;
using static CO.CDP.MQ.Outbox.Migrations;

#nullable disable

namespace CO.CDP.EntityVerification.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class OutboxNotificationTrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(NotificationFunction(name: "notify_of_outbox_message", schema: "entity_verification"));
            migrationBuilder.Sql(NotificationTrigger(
                name: "trigger_notify_of_outbox_message",
                function: "notify_of_outbox_message",
                table: "outbox_messages",
                channel: "entity_verification_outbox",
                schema: "entity_verification"
            ));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS trigger_notify_of_outbox_message ON entity_verification.outbox_messages");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS entity_verification.notify_of_outbox_message");
        }
    }
}
