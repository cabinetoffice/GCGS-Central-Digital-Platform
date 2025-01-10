using AutoMapper;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;
using Persistence = CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class SignOrganisationMouUseCase(
    Persistence.IOrganisationRepository organisationRepository,
    Persistence.IPersonRepository personRepository
    )
    : IUseCase<(Guid organisationId, SignMouRequest signMouRequest), bool>
{
    public async Task<bool> Execute((Guid organisationId, SignMouRequest signMouRequest) command)
    {
        var organisation = await organisationRepository.Find(command.organisationId)
                          ?? throw new UnknownOrganisationException($"Unknown organisation {command.organisationId}.");

        var person = await personRepository.Find(command.signMouRequest.CreatedById)
                           ?? throw new UnknownPersonException($"Unknown person {command.signMouRequest.CreatedById}.");

        var mou = await organisationRepository.GetMou(command.signMouRequest.MouId)
                   ?? throw new UnknownMouException($"Unknown Mou {command.signMouRequest.MouId}.");


        Persistence.MouSignature mouSignature = new Persistence.MouSignature
        {
            MouId = mou.Id,
            Mou = mou,
            CreatedById = person.Id,
            CreatedBy = person,
            OrganisationId = organisation.Id,
            Organisation = organisation,
            Name = command.signMouRequest.Name,
            JobTitle = command.signMouRequest.JobTitle,
            SignatureGuid = Guid.NewGuid()
        };

        organisationRepository.SaveOrganisationMou(mouSignature);

        return await Task.FromResult(true);
    }
}
