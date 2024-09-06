using AutoMapper;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;
using PersistenceConnectedEntity = CO.CDP.OrganisationInformation.Persistence.ConnectedEntity;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class UpdateConnectedEntityUseCase(
    IConnectedEntityRepository connectedEntityRepository,
    IOrganisationRepository organisationRepository,
    IMapper mapper)
    : IUseCase<(Guid organisationId, Guid connectedEntityId, UpdateConnectedEntity updateConnectedEntity), bool>
{
    public async Task<bool> Execute((Guid organisationId, Guid connectedEntityId, UpdateConnectedEntity updateConnectedEntity) command)
    {
        var organisation = await organisationRepository.Find(command.organisationId)
                                   ?? throw new UnknownOrganisationException($"Unknown organisation {command.organisationId}.");

        var connectedEntity = await connectedEntityRepository.Find(organisation.Guid, command.connectedEntityId)
                              ?? throw new UnknownConnectedEntityException(
                                  $"Unknown connected entity {command.connectedEntityId}.");

        connectedEntity = mapper.Map(command.updateConnectedEntity, connectedEntity);

        var newRegisteredAddress = command.updateConnectedEntity.Addresses.FirstOrDefault(a => a.Type == OrganisationInformation.AddressType.Registered);
        var existingRegisteredAddress = connectedEntity.Addresses.FirstOrDefault(a => a.Type == OrganisationInformation.AddressType.Registered);
        AddressUpdate(connectedEntity, newRegisteredAddress, existingRegisteredAddress);

        var newPostalAddress = command.updateConnectedEntity.Addresses.FirstOrDefault(a => a.Type == OrganisationInformation.AddressType.Postal);
        var existingPostalAddress = connectedEntity.Addresses.FirstOrDefault(a => a.Type == OrganisationInformation.AddressType.Postal);
        AddressUpdate(connectedEntity, newPostalAddress, existingPostalAddress);

        await connectedEntityRepository.Save(connectedEntity);

        return await Task.FromResult(true);
    }

    private void AddressUpdate(
        PersistenceConnectedEntity connectedEntity,
        OrganisationInformation.Address? updatedAddress,
        PersistenceConnectedEntity.ConnectedEntityAddress? existingAddress)
    {
        if (existingAddress != null)
        {
            if (updatedAddress == null)
            {
                connectedEntity.Addresses.Remove(existingAddress);
            }
            else
            {
                existingAddress.Address = mapper.Map(updatedAddress, existingAddress.Address);
            }
        }
        else if (updatedAddress != null)
        {
            connectedEntity.Addresses.Add(mapper.Map<PersistenceConnectedEntity.ConnectedEntityAddress>(updatedAddress));
        }
    }
}