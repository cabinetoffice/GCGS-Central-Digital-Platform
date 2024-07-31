namespace CO.CDP.Organisation.WebApi.UseCase;

public interface IUseCase<in TCommand, TOutcome>
{
    public Task<TOutcome> Execute(TCommand command);
}

public interface IUseCase<TOutcome>
{
    public Task<TOutcome> Execute();
}