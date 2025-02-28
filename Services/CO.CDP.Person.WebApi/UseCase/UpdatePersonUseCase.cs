using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.Person.WebApi.Model;

namespace CO.CDP.Person.WebApi.UseCase;

public class UpdatePersonUseCase(
    IPersonRepository personRepository    
   )
    : IUseCase<(Guid personId, UpdatePerson updatePerson), bool>
{
    public async Task<bool> Execute((Guid personId, UpdatePerson updatePerson) command)
    {
        var person = await personRepository.Find(command.personId)
          ?? throw new UnknownPersonException($"Unknown person {command.personId}.");

        person.PreviousUrns.Add(person.UserUrn);
        person.UserUrn = command.updatePerson.UserUrn;

        personRepository.Save(person);

        return await Task.FromResult(true);
    }
}