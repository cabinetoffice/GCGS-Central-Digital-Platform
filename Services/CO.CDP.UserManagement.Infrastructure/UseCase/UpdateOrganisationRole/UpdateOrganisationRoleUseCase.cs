using CO.CDP.Functional;
using CO.CDP.MQ;
using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Core.UseCase;
using CO.CDP.UserManagement.Infrastructure.Events;
using CO.CDP.UserManagement.Shared.Enums;
using Microsoft.Extensions.Logging;

namespace CO.CDP.UserManagement.Infrastructure.UseCase.UpdateOrganisationRole;

public record UpdateOrganisationRoleCommand(
    Guid CdpOrganisationId,
    Guid CdpPersonId,
    OrganisationRole NewRole,
    string ActingUserId);

public class UpdateOrganisationRoleUseCase(
    IOrganisationRepository organisationRepository,
    IUserOrganisationMembershipRepository membershipRepository,
    IRoleMappingService roleMappingService,
    IUnitOfWork unitOfWork,
    IPublisher publisher,
    ILogger<UpdateOrganisationRoleUseCase> logger) : IUseCase<UpdateOrganisationRoleCommand, UserOrganisationMembership>
{
    public async Task<UserOrganisationMembership> Execute(UpdateOrganisationRoleCommand command,
        CancellationToken ct = default)
    {
        var organisation = await organisationRepository.GetByCdpGuidAsync(command.CdpOrganisationId, ct)
                           ?? throw new EntityNotFoundException(nameof(Organisation), command.CdpOrganisationId);

        var membership = await membershipRepository.GetByPersonIdAndOrganisationAsync(
                             command.CdpPersonId, organisation.Id, ct)
                         ?? throw new EntityNotFoundException(nameof(UserOrganisationMembership), command.CdpPersonId);

        await roleMappingService.ApplyRoleDefinitionAsync(membership, command.NewRole, ct);
        membershipRepository.Update(membership);

        logger.LogInformation(
            "Role for user {CdpPersonId} in organisation {CdpOrganisationId} updated to {NewRole} by {ActingUser}",
            command.CdpPersonId, command.CdpOrganisationId, command.NewRole, command.ActingUserId);

        await Option.From(membership.CdpPersonId).TapAsync(async personId =>
        {
            var scopes = await roleMappingService.GetOrganisationInformationScopesAsync(membership.Id, ct);
            await publisher.Publish(new PersonScopesUpdated
            {
                OrganisationId = organisation.CdpOrganisationGuid.ToString(),
                PersonId = personId.ToString(),
                Scopes = scopes.ToList()
            });
        });

        await unitOfWork.SaveChangesAsync(ct);

        return membership;
    }
}