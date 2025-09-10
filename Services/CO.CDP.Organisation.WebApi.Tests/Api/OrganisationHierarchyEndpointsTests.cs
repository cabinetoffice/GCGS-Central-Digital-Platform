using System.Net;
using System.Net.Http.Json;
using CO.CDP.Authentication;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.TestKit.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CO.CDP.Organisation.WebApi.Tests.Api;

public class OrganisationHierarchyEndpointsTests
{
    private readonly Mock<ISupersedeChildOrganisationUseCase> _mockSupersedeChildOrganisationUseCase = new();
    private readonly Mock<IUseCase<CreateParentChildRelationshipRequest, CreateParentChildRelationshipResult>> _mockCreateParentChildRelationshipUseCase = new();
    private readonly Mock<IUseCase<Guid, GetChildOrganisationsResponse>> _mockGetChildOrganisationsUseCase = new();
    private readonly Mock<IUseCase<Guid, GetParentOrganisationsResponse>> _mockGetParentOrganisationsUseCase = new();

    private WebApplicationFactory<Program> CreateTestFactory(string authChannel, Guid organisationId, string? organisationPersonScope = null)
    {
        return new TestAuthorizationWebApplicationFactory<Program>(
            authChannel,
            organisationId,
            organisationPersonScope,
            services =>
            {
                services.AddSingleton(_mockCreateParentChildRelationshipUseCase.Object);
                services.AddSingleton(_mockGetChildOrganisationsUseCase.Object);
                services.AddSingleton(_mockSupersedeChildOrganisationUseCase.Object);
                services.AddSingleton(_mockGetParentOrganisationsUseCase.Object);
            });
    }

    #region Create Parent-Child Relationship Tests

    public static IEnumerable<object[]> ValidCreateParentChildRelationshipData()
    {
        yield return [Constants.Channel.OneLogin, Constants.OrganisationPersonScope.Admin];
        yield return [Constants.Channel.OneLogin, Constants.OrganisationPersonScope.Editor];
    }

    public static IEnumerable<object[]> InvalidCreateParentChildRelationshipData()
    {
        yield return [Constants.Channel.OneLogin, Constants.OrganisationPersonScope.Viewer];
        yield return [Constants.Channel.OneLogin, Constants.OrganisationPersonScope.Responder];
        yield return [Constants.Channel.ServiceKey, Constants.OrganisationPersonScope.Admin];
        yield return [Constants.Channel.OrganisationKey, Constants.OrganisationPersonScope.Admin];
    }

    [Theory]
    [MemberData(nameof(ValidCreateParentChildRelationshipData))]
    public async Task CreateParentChildRelationship_WithValidRoles_ReturnsCreated(string authChannel, string organisationPersonScope)
    {
        var organisationId = Guid.NewGuid();
        var factory = CreateTestFactory(authChannel, organisationId, organisationPersonScope);
        var client = factory.CreateClient();

        var childOrganisationId = Guid.NewGuid();
        var relationshipId = Guid.NewGuid();

        var request = new CreateParentChildRelationshipRequest
        {
            ParentId = organisationId,
            ChildId = childOrganisationId
        };

        var expectedResult = new CreateParentChildRelationshipResult
        {
            Success = true,
            RelationshipId = relationshipId
        };

        _mockCreateParentChildRelationshipUseCase
            .Setup(x => x.Execute(It.IsAny<CreateParentChildRelationshipRequest>()))
            .ReturnsAsync(expectedResult);

        var response = await client.PostAsJsonAsync($"/organisations/{organisationId}/hierarchy/child", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(InvalidCreateParentChildRelationshipData))]
    public async Task CreateParentChildRelationship_WithInvalidRoles_ReturnsForbidden(string authChannel, string organisationPersonScope)
    {
        var organisationId = Guid.NewGuid();
        var factory = CreateTestFactory(authChannel, organisationId, organisationPersonScope);
        var client = factory.CreateClient();

        var childOrganisationId = Guid.NewGuid();

        var request = new CreateParentChildRelationshipRequest
        {
            ParentId = organisationId,
            ChildId = childOrganisationId
        };

        var response = await client.PostAsJsonAsync($"/organisations/{organisationId}/hierarchy/child", request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateParentChildRelationship_WhenUseCaseFails_ReturnsBadRequest()
    {
        var organisationId = Guid.NewGuid();
        var factory = CreateTestFactory(Constants.Channel.OneLogin, organisationId, Constants.OrganisationPersonScope.Admin);
        var client = factory.CreateClient();

        var childOrganisationId = Guid.NewGuid();

        var request = new CreateParentChildRelationshipRequest
        {
            ParentId = organisationId,
            ChildId = childOrganisationId
        };

        _mockCreateParentChildRelationshipUseCase
            .Setup(x => x.Execute(It.Is<CreateParentChildRelationshipRequest>(r =>
                r.ParentId == organisationId && r.ChildId == childOrganisationId)))
            .ReturnsAsync(new CreateParentChildRelationshipResult { Success = false });

        var response = await client.PostAsJsonAsync($"/organisations/{organisationId}/hierarchy/child", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Equal("Bad Request", problemDetails.Title);
    }

    #endregion

    #region Get Child Organisations Tests

    public static IEnumerable<object[]> ValidGetChildOrganisationsData()
    {
        yield return [Constants.Channel.OneLogin, Constants.OrganisationPersonScope.Admin];
        yield return [Constants.Channel.OneLogin, Constants.OrganisationPersonScope.Editor];
        yield return [Constants.Channel.OneLogin, Constants.OrganisationPersonScope.Viewer];
        yield return [Constants.Channel.ServiceKey, Constants.OrganisationPersonScope.Admin];
        yield return [Constants.Channel.ServiceKey, Constants.OrganisationPersonScope.Editor];
        yield return [Constants.Channel.ServiceKey, Constants.OrganisationPersonScope.Viewer];
    }

    public static IEnumerable<object[]> InvalidGetChildOrganisationsData()
    {
        yield return [Constants.Channel.OneLogin, Constants.OrganisationPersonScope.Responder];
        yield return [Constants.Channel.OrganisationKey, Constants.OrganisationPersonScope.Admin];
    }

    [Theory]
    [MemberData(nameof(ValidGetChildOrganisationsData))]
    public async Task GetChildOrganisations_WithValidRoles_ReturnsOk(string authChannel, string organisationPersonScope)
    {
        var organisationId = Guid.NewGuid();
        var factory = CreateTestFactory(authChannel, organisationId, organisationPersonScope);
        var client = factory.CreateClient();

        var childOrganisations = new List<OrganisationSummary>
        {
            new() { Id = Guid.NewGuid(), Name = "Child Organisation 1" },
            new() { Id = Guid.NewGuid(), Name = "Child Organisation 2" }
        };

        var expectedResponse = new GetChildOrganisationsResponse
        {
            Success = true,
            ChildOrganisations = childOrganisations
        };

        _mockGetChildOrganisationsUseCase
            .Setup(x => x.Execute(It.IsAny<Guid>()))
            .ReturnsAsync(expectedResponse);

        var response = await client.GetAsync($"/organisations/{organisationId}/hierarchy/children");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(InvalidGetChildOrganisationsData))]
    public async Task GetChildOrganisations_WithInvalidRoles_ReturnsForbidden(string authChannel, string organisationPersonScope)
    {
        var organisationId = Guid.NewGuid();
        var factory = CreateTestFactory(authChannel, organisationId, organisationPersonScope);
        var client = factory.CreateClient();

        var response = await client.GetAsync($"/organisations/{organisationId}/hierarchy/children");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetChildOrganisations_WhenUseCaseFails_ReturnsInternalServerError()
    {
        var organisationId = Guid.NewGuid();
        var factory = CreateTestFactory(Constants.Channel.OneLogin, organisationId, Constants.OrganisationPersonScope.Admin);
        var client = factory.CreateClient();

        _mockGetChildOrganisationsUseCase
            .Setup(x => x.Execute(It.IsAny<Guid>()))
            .ReturnsAsync(new GetChildOrganisationsResponse { Success = false });

        var response = await client.GetAsync($"/organisations/{organisationId}/hierarchy/children");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Equal("An error occurred while processing your request.", problemDetails.Title);
    }

    #endregion

    #region Supersede Child Organisation Tests

    public static IEnumerable<object[]> ValidSupersedeChildOrganisationData()
    {
        yield return [Constants.Channel.OneLogin, Constants.OrganisationPersonScope.Admin];
        yield return [Constants.Channel.OneLogin, Constants.OrganisationPersonScope.Editor];
    }

    public static IEnumerable<object[]> InvalidSupersedeChildOrganisationData()
    {
        yield return [Constants.Channel.OneLogin, Constants.OrganisationPersonScope.Viewer];
        yield return [Constants.Channel.OneLogin, Constants.OrganisationPersonScope.Responder];
        yield return [Constants.Channel.ServiceKey, Constants.OrganisationPersonScope.Admin];
        yield return [Constants.Channel.OrganisationKey, Constants.OrganisationPersonScope.Admin];
    }

    [Theory]
    [MemberData(nameof(ValidSupersedeChildOrganisationData))]
    public async Task SupersedeChildOrganisation_WithValidRoles_ReturnsNoContent(string authChannel, string organisationPersonScope)
    {
        var organisationId = Guid.NewGuid();
        var factory = CreateTestFactory(authChannel, organisationId, organisationPersonScope);
        var client = factory.CreateClient();

        var childOrganisationId = Guid.NewGuid();

        var expectedResult = new SupersedeChildOrganisationResult
        {
            Success = true,
            NotFound = false
        };

        _mockSupersedeChildOrganisationUseCase
            .Setup(x => x.Execute(It.IsAny<SupersedeChildOrganisationRequest>()))
            .ReturnsAsync(expectedResult);

        var response = await client.DeleteAsync($"/organisations/{organisationId}/hierarchy/child/{childOrganisationId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(InvalidSupersedeChildOrganisationData))]
    public async Task SupersedeChildOrganisation_WithInvalidRoles_ReturnsForbidden(string authChannel, string organisationPersonScope)
    {
        var organisationId = Guid.NewGuid();
        var factory = CreateTestFactory(authChannel, organisationId, organisationPersonScope);
        var client = factory.CreateClient();
        var childOrganisationId = Guid.NewGuid();

        _mockSupersedeChildOrganisationUseCase
            .Setup(x => x.Execute(It.IsAny<SupersedeChildOrganisationRequest>()))
            .ReturnsAsync(new SupersedeChildOrganisationResult { Success = true });

        var response = await client.DeleteAsync($"/organisations/{organisationId}/hierarchy/child/{childOrganisationId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task SupersedeChildOrganisation_WhenRelationshipNotFound_ReturnsNotFound()
    {
        var organisationId = Guid.NewGuid();
        var factory = CreateTestFactory(Constants.Channel.OneLogin, organisationId, Constants.OrganisationPersonScope.Admin);
        var client = factory.CreateClient();
        var childOrganisationId = Guid.NewGuid();

        _mockSupersedeChildOrganisationUseCase
            .Setup(x => x.Execute(It.Is<SupersedeChildOrganisationRequest>(c => c.ParentOrganisationId == organisationId && c.ChildOrganisationId == childOrganisationId)))
            .ReturnsAsync(new SupersedeChildOrganisationResult { Success = false, NotFound = true });

        var response = await client.DeleteAsync($"/organisations/{organisationId}/hierarchy/child/{childOrganisationId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Equal("Not Found", problemDetails.Title);
    }

    [Fact]
    public async Task SupersedeChildOrganisation_WhenUseCaseFails_ReturnsBadRequest()
    {
        var organisationId = Guid.NewGuid();
        var factory = CreateTestFactory(Constants.Channel.OneLogin, organisationId, Constants.OrganisationPersonScope.Admin);
        var client = factory.CreateClient();
        var childOrganisationId = Guid.NewGuid();

        _mockSupersedeChildOrganisationUseCase
            .Setup(x => x.Execute(It.Is<SupersedeChildOrganisationRequest>(c => c.ParentOrganisationId == organisationId && c.ChildOrganisationId == childOrganisationId)))
            .ReturnsAsync(new SupersedeChildOrganisationResult { Success = false, NotFound = false });

        var response = await client.DeleteAsync($"/organisations/{organisationId}/hierarchy/child/{childOrganisationId}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Equal("Bad Request", problemDetails.Title);
    }
    #endregion

    #region Get Parent Organisations Tests

    public static IEnumerable<object[]> ValidGetParentOrganisationsData()
    {
        yield return [Constants.Channel.OneLogin, Constants.OrganisationPersonScope.Admin];
        yield return [Constants.Channel.OneLogin, Constants.OrganisationPersonScope.Editor];
        yield return [Constants.Channel.OneLogin, Constants.OrganisationPersonScope.Viewer];
        yield return [Constants.Channel.ServiceKey, Constants.OrganisationPersonScope.Admin];
        yield return [Constants.Channel.ServiceKey, Constants.OrganisationPersonScope.Editor];
        yield return [Constants.Channel.ServiceKey, Constants.OrganisationPersonScope.Viewer];
    }

    public static IEnumerable<object[]> InvalidGetParentOrganisationsData()
    {
        yield return [Constants.Channel.OneLogin, Constants.OrganisationPersonScope.Responder];
        yield return [Constants.Channel.OrganisationKey, Constants.OrganisationPersonScope.Admin];
    }

    [Theory]
    [MemberData(nameof(ValidGetParentOrganisationsData))]
    public async Task GetParentOrganisations_WithValidRoles_ReturnsOk(string authChannel, string organisationPersonScope)
    {
        var organisationId = Guid.NewGuid();
        var factory = CreateTestFactory(authChannel, organisationId, organisationPersonScope);
        var client = factory.CreateClient();

        var parentOrganisations = new List<OrganisationSummary>
        {
            new() { Id = Guid.NewGuid(), Name = "Parent Organisation 1" },
            new() { Id = Guid.NewGuid(), Name = "Parent Organisation 2" }
        };

        var expectedResponse = new GetParentOrganisationsResponse
        {
            Success = true,
            ParentOrganisations = parentOrganisations
        };

        _mockGetParentOrganisationsUseCase
            .Setup(x => x.Execute(organisationId))
            .ReturnsAsync(expectedResponse);

        var response = await client.GetAsync($"/organisations/{organisationId}/hierarchy/parent");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(InvalidGetParentOrganisationsData))]
    public async Task GetParentOrganisations_WithInvalidRoles_ReturnsForbidden(string authChannel, string organisationPersonScope)
    {
        var organisationId = Guid.NewGuid();
        var factory = CreateTestFactory(authChannel, organisationId, organisationPersonScope);
        var client = factory.CreateClient();

        var response = await client.GetAsync($"/organisations/{organisationId}/hierarchy/parent");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetParentOrganisations_WhenUseCaseFails_ReturnsInternalServerError()
    {
        var organisationId = Guid.NewGuid();
        var factory = CreateTestFactory(Constants.Channel.OneLogin, organisationId, Constants.OrganisationPersonScope.Admin);
        var client = factory.CreateClient();

        _mockGetParentOrganisationsUseCase
            .Setup(x => x.Execute(It.IsAny<Guid>()))
            .ReturnsAsync(new GetParentOrganisationsResponse { Success = false });

        var response = await client.GetAsync($"/organisations/{organisationId}/hierarchy/parent");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Equal("An error occurred while processing your request.", problemDetails.Title);
    }

    #endregion
}
