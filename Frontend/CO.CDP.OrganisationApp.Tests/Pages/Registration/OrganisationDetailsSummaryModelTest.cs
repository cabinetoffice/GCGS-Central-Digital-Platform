using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Registration;
using CO.CDP.Person.WebApiClient;
using CO.CDP.Tenant.WebApiClient;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Registration;

public class OrganisationDetailsSummaryModelTest
{
    private readonly Mock<ISession> sessionMock;
    private readonly Mock<IOrganisationClient> organisationClientMock;
    private readonly Mock<IPersonClient> personClientMock;
    private readonly Mock<ITenantClient> tenantClientMock;

    public OrganisationDetailsSummaryModelTest()
    {
        sessionMock = new Mock<ISession>();
        organisationClientMock = new Mock<IOrganisationClient>();
        personClientMock = new Mock<IPersonClient>();
        tenantClientMock = new Mock<ITenantClient>();
    }

    [Fact]
    public void OnGet_ValidSession_ReturnsRegistrationDetails()
    {
        var model = GivenOrganisationDetailModel();

        RegistrationDetails registrationDetails = DummyRegistrationDetails();

        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        model.OnGet();
        model.Details.As<RegistrationDetails>().Should().NotBeNull();
    }

    private RegistrationDetails DummyRegistrationDetails()
    {
        var registrationDetails = new RegistrationDetails
        {
            OrganisationName = "TestOrg",
            OrganisationScheme = "TestType",
            OrganisationEmailAddress = "test@example.com"
        };

        return registrationDetails;
    }

    [Fact]
    public void OnGet_InvalidSession_ThrowsException()
    {
        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(value: null);

        var model = GivenOrganisationDetailModel();

        Action action = () => model.OnGet();
        action.Should().Throw<Exception>().WithMessage("Shoudn't be here");
    }

    [Fact]
    public async Task OnPost_InvalidSession_ThrowsException()
    {
        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(value: null);

        var model = GivenOrganisationDetailModel();

        Func<Task> act = model.OnPost;
        await act.Should().ThrowAsync<Exception>().WithMessage("Shoudn't be here");
    }

    [Fact]
    public async Task OnPost_ValidSession_CallsCreateOrganisationAndPerson()
    {
        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey))
            .Returns(DummyRegistrationDetails());

        organisationClientMock.Setup(o => o.CreateOrganisationAsync(It.IsAny<NewOrganisation>()))
            .ReturnsAsync(GivenOrganisationClientModel());

        var model = GivenOrganisationDetailModel();

        await model.OnPost();

        organisationClientMock.Verify(o => o.CreateOrganisationAsync(It.IsAny<NewOrganisation>()), Times.Once);
        personClientMock.Verify(o => o.CreatePersonAsync(It.IsAny<NewPerson>()), Times.Once);
    }

    [Fact]
    public async Task OnPost_WhenErrorInRegisteringOrganisation_ShouldReturnPageWithError()
    {
        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey))
            .Returns(DummyRegistrationDetails());

        organisationClientMock.Setup(o => o.CreateOrganisationAsync(It.IsAny<NewOrganisation>()))
            .ThrowsAsync(new Organisation.WebApiClient.ApiException("Unexpected error", 500, "", default, null));

        var model = GivenOrganisationDetailModel();

        var actionResult = await model.OnPost();

        personClientMock.Verify(o => o.CreatePersonAsync(It.IsAny<NewPerson>()), Times.Never);
        actionResult.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnPost_WhenOrganisationAlreadyRegistered_ShouldNotRegisterOrganisationAgain()
    {
        var details = DummyRegistrationDetails();
        details.OrganisationId = Guid.NewGuid();

        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey))
            .Returns(details);

        organisationClientMock.Setup(o => o.GetOrganisationAsync(It.IsAny<Guid>()))
            .ReturnsAsync(GivenOrganisationClientModel());

        var model = GivenOrganisationDetailModel();

        var actionResult = await model.OnPost();

        organisationClientMock.Verify(o => o.CreateOrganisationAsync(It.IsAny<NewOrganisation>()), Times.Never);
        organisationClientMock.Verify(o => o.GetOrganisationAsync(It.IsAny<Guid>()), Times.Once);
        personClientMock.Verify(o => o.CreatePersonAsync(It.IsAny<NewPerson>()), Times.Once);
        actionResult.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("OrganisationAccount");
    }

    [Fact]
    public async Task OnPost_OnSuccess_RedirectsToOrganisationAccount()
    {
        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey))
            .Returns(DummyRegistrationDetails());

        organisationClientMock.Setup(o => o.CreateOrganisationAsync(It.IsAny<NewOrganisation>()))
            .ReturnsAsync(GivenOrganisationClientModel());

        var model = GivenOrganisationDetailModel();

        var actionResult = await model.OnPost();

        actionResult.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("OrganisationAccount");
    }

    //[Fact]
    //public async Task OnPost_WithValidSession_AssignsUserToOrganisation()
    //{
    //    var registrationDetails = DummyRegistrationDetails();
    //    var organisation = GivenOrganisationClientModel();
    //    var person = GivenPersonClientModel();

    //    sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey))
    //        .Returns(registrationDetails);
    //    organisationClientMock.Setup(o => o.CreateOrganisationAsync(It.IsAny<NewOrganisation>()))
    //        .ReturnsAsync(GivenOrganisationClientModel());
    //    personClientMock.Setup(p => p.CreatePersonAsync(It.IsAny<RegisterPerson>()))
    //        .ReturnsAsync(person);

    //    var model = GivenOrganisationDetailModel();
    //    await model.OnPost();

    //    tenantClientMock.Verify(t => t.AssignUserToOrganisationAsync(
    //        registrationDetails.TenantId.ToString(),
    //        It.Is<AssignUserToOrganisation>(au =>
    //            au.OrganisationId == organisation.Id &&
    //            au.PersonId == person.Id)),
    //        Times.Once);
    //}

    private Person.WebApiClient.Person GivenPersonClientModel()
    {
        return new Person.WebApiClient.Person(null, "test@gmail.com", "John", Guid.NewGuid(), "Smith");
    }

    private Organisation.WebApiClient.Organisation GivenOrganisationClientModel()
    {
        return new Organisation.WebApiClient.Organisation(null, null, null, Guid.NewGuid(), null, "Test Org", []);
    }

    private OrganisationDetailsSummaryModel GivenOrganisationDetailModel()
    {
        return new OrganisationDetailsSummaryModel(sessionMock.Object, organisationClientMock.Object, personClientMock.Object, );
    }
}
