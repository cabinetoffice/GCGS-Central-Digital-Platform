namespace CO.CDP.MQ.Outbox;

public static class Migrations
{
    public static string NotificationFunction(string name = "notify_of_outbox_message") =>
        $"""
         CREATE OR REPLACE FUNCTION {name}()
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
         """;

    public static string NotificationTrigger(
        string name = "trigger_notify_of_outbox_message",
        string function = "notify_of_outbox_message",
        string table = "outbox_messages",
        string channel = "outbox") =>
        $"""
         CREATE OR REPLACE TRIGGER {name}
         AFTER INSERT ON "{table}"
         FOR EACH ROW EXECUTE PROCEDURE {function}('{channel}');
         """;
}