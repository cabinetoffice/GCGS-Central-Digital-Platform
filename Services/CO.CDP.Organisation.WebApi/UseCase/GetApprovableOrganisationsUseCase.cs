// using CO.CDP.Organisation.WebApi.Model;
// using CO.CDP.OrganisationInformation;
// using CO.CDP.OrganisationInformation.Persistence;
//
// namespace CO.CDP.Organisation.WebApi.UseCase;
// public class GetApprovableOrganisationsUseCase(IOrganisationRepository organisationRepository)
//     : IUseCase<ApprovableOrganisationQuery, IEnumerable<Model.ApprovableOrganisation>>
// {
//     public async Task<IEnumerable<Model.OrganisationExtended>> Execute(ApprovableOrganisationQuery command)
//     {
//         var organisations = await organisationRepository.Get(command.Type);
//
//         return organisations.Select(organisation => new ApprovableOrganisation
//             {
//                 Id = organisation.Guid,
//                 Name = organisation.Name,
//                 Identifiers = GetIndentifiers(organisation),
//                 Email = GetEmail(organisation),
//                 Ppon = GetPpon(organisation),
//                 ApprovedOn = organisation.ApprovedOn,
//                 ApprovedById = organisation.ApprovedBy?.Guid,
//                 ApprovedByName = organisation.ApprovedBy?.FirstName + " " + organisation.ApprovedBy?.LastName,
//                 ApprovedComment = organisation.ApprovedComment
//             })
//             .ToList();
//     }
//
//     private static List<Identifier> GetIndentifiers(OrganisationInformation.Persistence.Organisation organisation)
//     {
//         return organisation.Identifiers.Select(identifier => new Identifier
//         {
//             Scheme = identifier.Scheme,
//             Id = identifier.IdentifierId,
//             LegalName = identifier.LegalName,
//             Uri = (identifier.Uri != null) ? new Uri(identifier.Uri) : null
//         }).ToList();
//     }
//
//     private static string? GetEmail(OrganisationInformation.Persistence.Organisation organisation)
//     {
//         return organisation.ContactPoints.FirstOrDefault()?.Email;
//     }
//     private static string? GetPpon(OrganisationInformation.Persistence.Organisation organisation)
//     {
//         var identifier = organisation.Identifiers.FirstOrDefault(i => i.Scheme == "CDP-PPON");
//         return identifier?.IdentifierId;
//     }
// }