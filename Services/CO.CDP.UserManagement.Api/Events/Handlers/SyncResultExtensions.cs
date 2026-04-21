using CO.CDP.Functional;
using CO.CDP.UserManagement.Core.Exceptions;

namespace CO.CDP.UserManagement.Api.Events.Handlers;

internal static class SyncResultExtensions
{
    internal static void ThrowOnFailure(this Result<string, Unit> result, string context, ILogger logger)
    {
        result.Match(
            onLeft: error =>
            {
                logger.LogError("Sync step failed ({Context}): {Error}", context, error);
                throw new SyncStepFailedException($"{context}: {error}");
            },
            onRight: _ => { });
    }
}