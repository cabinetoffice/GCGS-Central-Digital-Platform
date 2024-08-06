namespace CO.CDP.DataSharing.WebApi.UseCase;

public interface IUseCase<in TCommand, TOutcome>
{
    public Task<TOutcome> Execute(TCommand command);
}