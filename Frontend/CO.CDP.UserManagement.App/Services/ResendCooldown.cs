using System.Globalization;
using System.Text;

namespace CO.CDP.UserManagement.App.Services;

public static class ResendCooldown
{
    private const string KeyPrefix = "ResendCooldown:";

    public static bool IsAllowed(ISession session, Guid inviteGuid, TimeSpan cooldown) =>
        ElapsedSince(session, inviteGuid) is not { } elapsed || elapsed >= cooldown;

    public static void Record(ISession session, Guid inviteGuid) =>
        session.Set(KeyPrefix + inviteGuid, Encoding.UTF8.GetBytes(DateTimeOffset.UtcNow.ToString("O")));

    public static int GetRemainingSeconds(ISession session, Guid inviteGuid, TimeSpan cooldown)
    {
        var remaining = ElapsedSince(session, inviteGuid) is { } elapsed
            ? cooldown - elapsed
            : TimeSpan.Zero;

        return remaining > TimeSpan.Zero ? (int)Math.Ceiling(remaining.TotalSeconds) : 0;
    }

    private static TimeSpan? ElapsedSince(ISession session, Guid inviteGuid)
    {
        if (!session.TryGetValue(KeyPrefix + inviteGuid, out var bytes))
            return null;

        var raw = Encoding.UTF8.GetString(bytes);
        return DateTimeOffset.TryParse(raw, null, DateTimeStyles.RoundtripKind, out var recorded)
            ? DateTimeOffset.UtcNow - recorded
            : null;
    }
}