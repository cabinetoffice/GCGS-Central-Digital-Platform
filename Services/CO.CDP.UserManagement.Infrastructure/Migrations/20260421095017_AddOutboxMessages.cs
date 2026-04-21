using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CO.CDP.UserManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOutboxMessages : Migration
    {
        private const string Schema = "user_management";
        private const string TableName = "OutboxMessages";
        private const string NotificationFunction = "notify_of_user_management_outbox_message";
        private const string NotificationTrigger = "trigger_notify_of_user_management_outbox_message";
        private const string NotificationChannel = "user_management_outbox";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: TableName,
                schema: Schema,
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Published = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    QueueUrl = table.Column<string>(type: "text", nullable: false),
                    MessageGroupId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_CreatedOn",
                schema: Schema,
                table: TableName,
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Published",
                schema: Schema,
                table: TableName,
                column: "Published");

            migrationBuilder.Sql($"""
                CREATE OR REPLACE FUNCTION {Schema}.{NotificationFunction}()
                RETURNS TRIGGER AS $trigger$
                DECLARE
                  payload TEXT;
                  channel_name TEXT;
                BEGIN
                  IF TG_ARGV[0] IS NULL THEN
                    RAISE EXCEPTION 'A channel name is required as the first argument';
                  END IF;

                  channel_name := TG_ARGV[0];
                  payload := json_build_object('timestamp', CURRENT_TIMESTAMP, 'payload', row_to_json(NEW));

                  PERFORM pg_notify(channel_name, payload);

                  RETURN NEW;
                END;
                $trigger$ LANGUAGE plpgsql;
                """);

            migrationBuilder.Sql($"""
                CREATE OR REPLACE TRIGGER {NotificationTrigger}
                AFTER INSERT ON {Schema}."{TableName}"
                FOR EACH ROW EXECUTE PROCEDURE {Schema}.{NotificationFunction}('{NotificationChannel}');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"DROP TRIGGER IF EXISTS {NotificationTrigger} ON {Schema}.\"{TableName}\";");
            migrationBuilder.Sql($"DROP FUNCTION IF EXISTS {Schema}.{NotificationFunction}();");

            migrationBuilder.DropTable(
                name: TableName,
                schema: Schema);
        }
    }
}
