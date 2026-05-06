using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.Organisation.WebApi.ApplicationRegistry.Model;
using CO.CDP.ApplicationRegistry.Persistence.Repositories;

namespace CO.CDP.Organisation.WebApi.ApplicationRegistry.UseCase.Audit;

public record AuditQuery(
    string? EntityType,
    string? Action,
    string? UserId,
    DateTimeOffset? From,
    DateTimeOffset? To,
    int Limit = 100,
    int Offset = 0);

public class GetAuditLogsUseCase : IUseCase<AuditQuery, IEnumerable<AuditLogDto>>
{
    private readonly IAuditRepository _repository;

    public GetAuditLogsUseCase(IAuditRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<AuditLogDto>> Execute(AuditQuery query)
    {
        var logs = await _repository.GetAuditLogsAsync(
            query.EntityType, query.Action, query.UserId,
            query.From, query.To, query.Limit, query.Offset);

        return logs.Select(l => new AuditLogDto(
            l.Id, l.EntityType, l.EntityId, l.Action,
            l.PropertyName, l.OldValue, l.NewValue,
            l.UserId, l.Timestamp));
    }
}
