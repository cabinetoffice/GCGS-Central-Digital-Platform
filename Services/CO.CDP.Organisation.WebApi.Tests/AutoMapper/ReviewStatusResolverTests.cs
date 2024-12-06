using AutoMapper;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation;
using FluentAssertions;
using static CO.CDP.Organisation.WebApi.AutoMapper.WebApiToPersistenceProfile;

namespace CO.CDP.Organisation.WebApi.Tests.AutoMapper;
public class ReviewStatusResolverTests
{
    private readonly ReviewStatusResolver _resolver;

    public ReviewStatusResolverTests()
    {
        _resolver = new ReviewStatusResolver();
    }

    [Fact]
    public void Resolve_ShouldReturnApproved_WhenNoPendingRolesExist()
    {
        var organisation = GivenOrganisation();
        var review = new Review();

        var result = _resolver.Resolve(organisation, review, default, null!);

        result.Should().Be(ReviewStatus.Approved);
    }

    [Fact]
    public void Resolve_ShouldReturnPending_WhenPendingRolesExistAndReviewedByIsNull()
    {
        var organisation = GivenOrganisation(pendingRoles: [PartyRole.Buyer]);
        var review = new Review();

        var result = _resolver.Resolve(organisation, review, default, null!);

        result.Should().Be(ReviewStatus.Pending);
    }

    [Fact]
    public void Resolve_ShouldReturnRejected_WhenPendingRolesExistAndReviewedByIsNotNull()
    {
        var person = GivenPerson();
        var organisation = GivenOrganisation(pendingRoles: [PartyRole.Buyer], reviewedBy: person);
        var review = new Review();

        var result = _resolver.Resolve(organisation, review, default, null!);

        result.Should().Be(ReviewStatus.Rejected);
    }

    private static OrganisationInformation.Persistence.Organisation GivenOrganisation(List<PartyRole>? pendingRoles = null, OrganisationInformation.Persistence.Person? reviewedBy = null)
    {
        return new OrganisationInformation.Persistence.Organisation
        {
            Guid = Guid.NewGuid(),
            Tenant = new OrganisationInformation.Persistence.Tenant { Guid = new Guid(), Name = "Tenant" },
            Name = "Org name",
            Type = OrganisationType.Organisation,
            PendingRoles = (pendingRoles != null) ? pendingRoles : [],
            ReviewedBy = (reviewedBy != null) ? reviewedBy : null
        };
    }

    private static OrganisationInformation.Persistence.Person GivenPerson()
    {
        return new OrganisationInformation.Persistence.Person { Guid = new Guid(), Email = "asd@asd.com", FirstName = "First name", LastName = "Last name" };
    }
}
