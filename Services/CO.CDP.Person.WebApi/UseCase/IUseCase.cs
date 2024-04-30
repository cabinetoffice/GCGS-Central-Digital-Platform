namespace CO.CDP.Person.WebApi.UseCase;

public interface IUseCase<in TCommand, TOutcome>
{
    public Task<TOutcome> Execute(TCommand command);
}