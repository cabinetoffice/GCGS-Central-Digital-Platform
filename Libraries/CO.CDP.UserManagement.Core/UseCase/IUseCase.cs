namespace CO.CDP.UserManagement.Core.UseCase;

public interface IUseCase<in TCommand, TOutcome>
{
    Task<TOutcome> Execute(TCommand command, CancellationToken ct = default);
}

public interface IUseCase<in TCommand>
{
    Task Execute(TCommand command, CancellationToken ct = default);
}