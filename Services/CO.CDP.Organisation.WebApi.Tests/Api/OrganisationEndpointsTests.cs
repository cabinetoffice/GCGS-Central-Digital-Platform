using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation;
using CO.CDP.TestKit.Mvc;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using System.Net;
using System.Net.Http.Json;
using CO.CDP.MQ;
using static CO.CDP.Authentication.Constants;
using static System.Net.HttpStatusCode;

namespace CO.CDP.Organisation.WebApi.Tests.Api;
public class OrganisationEndpointsTests
{
    private readonly HttpClient _httpClient;
    private readonly Mock<IUseCase<RegisterOrganisation, Model.Organisation>> _registerOrganisationUseCase = new();
    private readonly Mock<IUseCase<PaginatedOrganisationQuery, Tuple<IEnumerable<OrganisationDto>, int>>> _getOrganisationsUseCase = new();
    private readonly Mock<IUseCase<Guid, Model.Organisation>> _getOrganisationUseCase = new();
    private readonly Mock<IUseCase<Guid, IEnumerable<Review>>> _getReviewsUseCase = new();
    private readonly Mock<IUseCase<(Guid, UpdateOrganisation), bool>> _updatesOrganisationUseCase = new();
    private readonly Mock<IUseCase<(Guid, OrganisationJoinRequestStatus?), IEnumerable<JoinRequestLookUp>>> _getOrganisationJoinRequestsUseCase = new();
    private readonly Mock<IUseCase<(Guid, Guid, UpdateJoinRequest), bool>> _updateJoinRequestUseCase = new();
    private readonly Mock<IUseCase<OrganisationSearchQuery, IEnumerable<OrganisationSearchResult>>> _searchOrganisationUseCase = new();
    private readonly Mock<IUseCase<OrganisationSearchByPponQuery, (IEnumerable<OrganisationSearchByPponResult>, int)>> _searchByNameOrPponUseCase = new();

    public OrganisationEndpointsTests()
    {
        TestWebApplicationFactory<Program> factory = new(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => _registerOrganisationUseCase.Object);
                services.AddScoped(_ => _getOrganisationsUseCase.Object);
                services.AddScoped(_ => _getOrganisationUseCase.Object);
                services.AddScoped(_ => _updatesOrganisationUseCase.Object);
                var mockPublisher = new Mock<IPublisher>();

                services.AddScoped(
                    sp => mockPublisher.Object);

                services.ConfigureFakePolicyEvaluator();
            });
        });
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task ItRegistersNewOrganisation()
    {
        var command = GivenRegisterOrganisationCommand();
        var organisation = SetupRegisterOrganisationUseCaseMock(command);

        var response = await _httpClient.PostAsJsonAsync("/organisations", command);

        response.Should().HaveStatusCode(Created, await response.Content.ReadAsStringAsync());

        var returnedOrganisation = await response.Content.ReadFromJsonAsync<Model.Organisation>();
        returnedOrganisation.Should().BeEquivalentTo(organisation, options => options.ComparingByMembers<Model.Organisation>());
    }

    [Fact]
    public async Task ItHandlesOrganisationCreationFailure()
    {
        var command = GivenRegisterOrganisationCommand();

        _registerOrganisationUseCase.Setup(useCase => useCase.Execute(It.IsAny<RegisterOrganisation>()))
                                    .ReturnsAsync((Model.Organisation)null!);

        var response = await _httpClient.PostAsJsonAsync("/organisations", command);

        response.Should().HaveStatusCode(HttpStatusCode.InternalServerError, await response.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineData(true, NoContent)]
    [InlineData(false, NotFound)]
    public async Task UpdateOrganisation_TestCases(bool useCaseResult, HttpStatusCode expectedStatusCode)
    {
        var organisationId = Guid.NewGuid();
        var updateOrganisation = new UpdateOrganisation { Type = OrganisationUpdateType.AdditionalIdentifiers, Organisation = new() };
        var command = (organisationId, updateOrganisation);

        if (useCaseResult)
            _updatesOrganisationUseCase.Setup(uc => uc.Execute(command)).ReturnsAsync(true);
        else
            _updatesOrganisationUseCase.Setup(uc => uc.Execute(command)).ThrowsAsync(new UnknownOrganisationException(""));

        var response = await _httpClient.PatchAsJsonAsync($"/organisations/{organisationId}", updateOrganisation);

        response.StatusCode.Should().Be(expectedStatusCode);
    }

    [Fact]
    public async Task UpdateOrganisation_InvalidOrganisationId_ReturnsUnprocessableEntity()
    {
        var invalidOrganisationId = "invalid-guid";

        var response = await _httpClient.PatchAsJsonAsync($"/organisations/{invalidOrganisationId}",
            new UpdateOrganisation { Type = OrganisationUpdateType.AdditionalIdentifiers, Organisation = new() });

        response.StatusCode.Should().Be(UnprocessableEntity);
    }

    [Theory]
    [InlineData(OK, Channel.OneLogin, PersonScope.SupportAdmin)]
    [InlineData(Forbidden, Channel.OneLogin)]
    [InlineData(Forbidden, Channel.ServiceKey)]
    [InlineData(Forbidden, Channel.OrganisationKey)]
    [InlineData(Forbidden, "unknown_channel")]
    public async Task GetAllOrganisations_Authorization_ReturnsExpectedStatusCode(
        HttpStatusCode expectedStatusCode, string channel, string? personScope = null)
    {
        _getOrganisationsUseCase.Setup(uc => uc.Execute(It.IsAny<PaginatedOrganisationQuery>()))
            .ReturnsAsync(new Tuple<IEnumerable<OrganisationDto>, int>([], 0));

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            channel,
            serviceCollection: s => s.AddScoped(_ => _getOrganisationsUseCase.Object),
            assignedPersonScopes: personScope);

        var response = await factory.CreateClient().GetAsync("/organisations?type=any&limit=10&skip=1");

        response.StatusCode.Should().Be(expectedStatusCode);
    }

    [Theory]
    [InlineData(Created, Channel.OneLogin)]
    [InlineData(Forbidden, Channel.ServiceKey)]
    [InlineData(Forbidden, Channel.OrganisationKey)]
    [InlineData(Forbidden, "unknown_channel")]
    public async Task CreateOrganisation_Authorization_ReturnsExpectedStatusCode(
        HttpStatusCode expectedStatusCode, string channel)
    {
        var command = GivenRegisterOrganisationCommand();
        SetupRegisterOrganisationUseCaseMock(command);
        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            channel, serviceCollection: s => s.AddScoped(_ => _registerOrganisationUseCase.Object));
        var httpClient = factory.CreateClient();

        var response = await httpClient.PostAsJsonAsync("/organisations", command);

        response.StatusCode.Should().Be(expectedStatusCode);
    }

    [Theory]
    [InlineData(OK, Channel.OneLogin, null, PersonScope.SupportAdmin)]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Admin)]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Editor)]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Viewer)]
    [InlineData(OK, Channel.ServiceKey)]
    [InlineData(Forbidden, Channel.OrganisationKey)]
    [InlineData(Forbidden, "unknown_channel")]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Responder)]
    public async Task GetOrganisation_Authorization_ReturnsExpectedStatusCode(
        HttpStatusCode expectedStatusCode, string channel, string? organisationPersonScope = null, string? personScope = null)
    {
        var organisationId = Guid.NewGuid();

        _getOrganisationUseCase.Setup(useCase => useCase.Execute(organisationId))
                                    .ReturnsAsync(GivenOrganisation(organisationId));

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            channel, organisationId, organisationPersonScope,
            services => services.AddScoped(_ => _getOrganisationUseCase.Object), personScope);

        var response = await factory.CreateClient().GetAsync($"/organisations/{organisationId}");

        response.StatusCode.Should().Be(expectedStatusCode);
    }

    [Theory]
    [InlineData(NoContent, Channel.OneLogin, OrganisationPersonScope.Admin)]
    [InlineData(NoContent, Channel.OneLogin, OrganisationPersonScope.Editor)]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Responder)]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Viewer)]
    [InlineData(Forbidden, Channel.ServiceKey)]
    [InlineData(Forbidden, Channel.OrganisationKey)]
    [InlineData(Forbidden, "unknown_channel")]
    public async Task UpdateOrganisation_Authorization_ReturnsExpectedStatusCode(
        HttpStatusCode expectedStatusCode, string channel, string? scope = null)
    {
        var organisationId = Guid.NewGuid();
        var updateOrganisation = new UpdateOrganisation { Type = OrganisationUpdateType.AdditionalIdentifiers, Organisation = new() };
        var command = (organisationId, updateOrganisation);

        _updatesOrganisationUseCase.Setup(uc => uc.Execute(command)).ReturnsAsync(true);

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            channel, organisationId, scope,
            services => services.AddScoped(_ => _updatesOrganisationUseCase.Object));

        var response = await factory.CreateClient().PatchAsJsonAsync($"/organisations/{organisationId}", updateOrganisation);

        response.StatusCode.Should().Be(expectedStatusCode);
    }

    [Theory]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Admin)]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Editor)]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Responder)]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Viewer)]
    [InlineData(Forbidden, Channel.ServiceKey)]
    [InlineData(Forbidden, Channel.OrganisationKey)]
    [InlineData(Forbidden, "unknown_channel")]
    public async Task GetOrganisationJoinRequests_Authorization_ReturnsExpectedStatusCode(
        HttpStatusCode expectedStatusCode, string channel, string? scope = null)
    {
        var organisationId = Guid.NewGuid();
        OrganisationJoinRequestStatus? status = OrganisationJoinRequestStatus.Pending;
        var command = (organisationId, status);

        _getOrganisationJoinRequestsUseCase.Setup(uc => uc.Execute(command)).ReturnsAsync([]);

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            channel, organisationId, scope,
            services => services.AddScoped(_ => _getOrganisationJoinRequestsUseCase.Object));

        var response = await factory.CreateClient().GetAsync($"/organisations/{organisationId}/join-requests?status={status}");

        response.StatusCode.Should().Be(expectedStatusCode);
    }

    [Theory]
    [InlineData(NoContent, Channel.OneLogin, OrganisationPersonScope.Admin)]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Editor)]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Responder)]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Viewer)]
    [InlineData(Forbidden, Channel.ServiceKey)]
    [InlineData(Forbidden, Channel.OrganisationKey)]
    [InlineData(Forbidden, "unknown_channel")]
    public async Task UpdateOrganisationJoinRequest_Authorization_ReturnsExpectedStatusCode(
        HttpStatusCode expectedStatusCode, string channel, string? scope = null)
    {
        var organisationId = Guid.NewGuid();
        var joinRequestId = Guid.NewGuid();
        var updateJoinRequest = new UpdateJoinRequest { ReviewedBy = Guid.NewGuid(), status = OrganisationJoinRequestStatus.Accepted };
        var command = (organisationId, joinRequestId, updateJoinRequest);

        _updateJoinRequestUseCase.Setup(uc => uc.Execute(command)).ReturnsAsync(true);

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            channel, organisationId, scope,
            services => services.AddScoped(_ => _updateJoinRequestUseCase.Object));

        var response = await factory.CreateClient().PatchAsJsonAsync($"/organisations/{organisationId}/join-requests/{joinRequestId}", updateJoinRequest);

        response.StatusCode.Should().Be(expectedStatusCode);
    }

    [Theory]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Admin)]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Editor)]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Viewer)]
    [InlineData(OK, Channel.ServiceKey)]
    [InlineData(Forbidden, Channel.OrganisationKey)]
    [InlineData(Forbidden, "unknown_channel")]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Responder)]
    public async Task GetOrganisationReviews_Authorization_ReturnsExpectedStatusCode(
        HttpStatusCode expectedStatusCode, string channel, string? scope = null)
    {
        var organisationId = Guid.NewGuid();

        var reviews = new List<Review> {
                new Review {
                    ApprovedOn = null,
                    ReviewedBy = new ReviewedBy { Id = new Guid(), Name = "Reviewer name"},
                    Comment = "Org name is wrong",
                    Status = ReviewStatus.Rejected
                }
            };

        _getReviewsUseCase.Setup(uc => uc.Execute(organisationId))
            .ReturnsAsync(reviews);

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            channel, organisationId, scope,
            services => services.AddScoped(_ => _getReviewsUseCase.Object));

        var response = await factory.CreateClient().GetAsync($"/organisations/{organisationId}/reviews");

        response.StatusCode.Should().Be(expectedStatusCode);

        if(expectedStatusCode != Forbidden)
        {
            var returnedReviews = await response.Content.ReadFromJsonAsync<List<Review>>();
            returnedReviews.Should().BeEquivalentTo(reviews, options => options.ComparingByMembers<Review>());
        }
    }

    [Fact]
    public async Task GetOrganisationReviews_Returns404_WhenThereAreNoReviews()
    {
        var organisationId = Guid.NewGuid();

        _getReviewsUseCase.Setup(uc => uc.Execute(organisationId))
            .ReturnsAsync(new List<Review>());

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            Channel.OneLogin, organisationId, OrganisationPersonScope.Editor,
            services => services.AddScoped(_ => _getReviewsUseCase.Object));

        var response = await factory.CreateClient().GetAsync($"/organisations/{organisationId}/reviews");

        response.StatusCode.Should().Be(NotFound);
    }

    [Theory]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Admin)]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Editor)]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Viewer)]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Responder)]
    [InlineData(OK, Channel.ServiceKey)]
    [InlineData(Forbidden, Channel.OrganisationKey)]
    [InlineData(Forbidden, "unknown_channel")]
    public async Task GetOrganisationSearch_Authorization_ReturnsExpectedStatusCode(
        HttpStatusCode expectedStatusCode, string channel, string? scope = null)
    {
        var organisationId = Guid.NewGuid();

        var searchResults = new List<OrganisationSearchResult>
        {
            new OrganisationSearchResult
            {
                Id = Guid.NewGuid(),
                Identifier = new Identifier
                {
                    Scheme = "GB-PPON",
                    Id = "123",
                    LegalName = "legal name"
                },
                Name = "Org name",
                Roles = new List<PartyRole>
                {
                    PartyRole.Buyer
                },
                Type = OrganisationType.Organisation
            }
        };

        _searchOrganisationUseCase.Setup(uc => uc.Execute(It.IsAny<OrganisationSearchQuery>()))
            .ReturnsAsync(searchResults);

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            channel, organisationId, scope,
            services => services.AddScoped(_ => _searchOrganisationUseCase.Object));

        var response = await factory.CreateClient().GetAsync($"/organisation/search?name=asd&limit=10");

        response.StatusCode.Should().Be(expectedStatusCode);

        if (expectedStatusCode != Forbidden)
        {
            var results = await response.Content.ReadFromJsonAsync<List<OrganisationSearchResult>>();
            results.Should().BeEquivalentTo(searchResults, options => options.ComparingByMembers<OrganisationSearchResult>());
        }
    }

    [Fact]
    public async Task GetOrganisationSearch_ReturnsBadRequest_WhenThresholdIsBelowZero()
    {
        var organisationId = Guid.NewGuid();

        _searchOrganisationUseCase.Setup(uc => uc.Execute(It.IsAny<OrganisationSearchQuery>()))
            .ReturnsAsync(new List<OrganisationSearchResult>());

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            Channel.OneLogin, organisationId, OrganisationPersonScope.Viewer,
            services => services.AddScoped(_ => _searchOrganisationUseCase.Object));

        var response = await factory.CreateClient().GetAsync($"/organisation/search?name=asd&limit=10&threshold=-1");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetOrganisationSearch_ReturnsBadRequest_WhenThresholdIsAboveOne()
    {
        var organisationId = Guid.NewGuid();

        _searchOrganisationUseCase.Setup(uc => uc.Execute(It.IsAny<OrganisationSearchQuery>()))
            .ReturnsAsync(new List<OrganisationSearchResult>());

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            Channel.OneLogin, organisationId, OrganisationPersonScope.Viewer,
            services => services.AddScoped(_ => _searchOrganisationUseCase.Object));

        var response = await factory.CreateClient().GetAsync($"/organisation/search?name=asd&limit=10&threshold=2");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SearchByNameOrPpon_ReturnsResults_WhenResultsAreFound()
    {
        var organisationId = Guid.NewGuid();

        var searchResults = new List<OrganisationSearchByPponResult>
        {
            new OrganisationSearchByPponResult
            {
                Id = Guid.NewGuid(),
                Name = "Test Organisation",
                Type = OrganisationType.Organisation,
                Identifiers = new List<Identifier>
                {
                    new Identifier
                    {
                        Scheme = "GB-PPON",
                        Id = "PGWZ-1758-ABCD",
                        LegalName = "Test Organisation Legal Name"
                    }
                },
                Roles = new List<PartyRole> { PartyRole.Buyer },
                Addresses = new List<OrganisationAddress>()
            },
            new OrganisationSearchByPponResult
            {
                Id = Guid.NewGuid(),
                Name = "Another Organisation",
                Type = OrganisationType.Organisation,
                Identifiers = new List<Identifier>
                {
                    new Identifier
                    {
                        Scheme = "GB-PPON",
                        Id = "PGWZ-1758-EFGH",
                        LegalName = "Another Organisation Legal Name"
                    }
                },
                Roles = new List<PartyRole> { PartyRole.Tenderer },
                Addresses = new List<OrganisationAddress>()
            }
        };

        int totalCount = 2;
        var searchResponse = (searchResults, totalCount);

        _searchByNameOrPponUseCase.Setup(uc => uc.Execute(It.IsAny<OrganisationSearchByPponQuery>()))
            .ReturnsAsync(searchResponse);

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            Channel.OneLogin, organisationId, OrganisationPersonScope.Admin,
            services => services.AddScoped(_ => _searchByNameOrPponUseCase.Object));

        var response = await factory.CreateClient().GetAsync($"/organisation/search-by-name-or-ppon?searchText=test&limit=10&skip=0&sortOrder=asc");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<OrganisationSearchByPponResponse>();
        content.Should().NotBeNull();
        content!.Results.Should().HaveCount(2);
        content.TotalCount.Should().Be(2);
        content.Results.Should().BeEquivalentTo(searchResults, options => options.ComparingByMembers<OrganisationSearchByPponResult>());
    }

    [Fact]
    public async Task SearchByNameOrPpon_ReturnsNotFound_WhenNoResultsAreFound()
    {
        var organisationId = Guid.NewGuid();

        var searchResults = new List<OrganisationSearchByPponResult>();
        int totalCount = 0;
        var searchResponse = (searchResults, totalCount);

        _searchByNameOrPponUseCase.Setup(uc => uc.Execute(It.IsAny<OrganisationSearchByPponQuery>()))
            .ReturnsAsync(searchResponse);

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            Channel.OneLogin, organisationId, OrganisationPersonScope.Admin,
            services => services.AddScoped(_ => _searchByNameOrPponUseCase.Object));

        var response = await factory.CreateClient().GetAsync($"/organisation/search-by-name-or-ppon?searchText=nonexistent&limit=10&skip=0&sortOrder=asc");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SearchByNameOrPpon_ReturnsUnprocessableContent_WhenRequiredParameterIsMissing()
    {
        var organisationId = Guid.NewGuid();

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            Channel.OneLogin, organisationId, OrganisationPersonScope.Admin,
            services => services.AddScoped(_ => _searchByNameOrPponUseCase.Object));

        var response = await factory.CreateClient().GetAsync($"/organisation/search-by-name-or-ppon?limit=10&skip=0&sortOrder=asc");

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableContent);
    }

    [Theory]
    [InlineData("invalid", HttpStatusCode.OK)]
    [InlineData("asecending", HttpStatusCode.OK)]
    public async Task SearchByNameOrPpon_AcceptsInvalidSortOrder_AndReturnsData(string sortOrder, HttpStatusCode expectedStatusCode)
    {
        var organisationId = Guid.NewGuid();

        var searchResults = new List<OrganisationSearchByPponResult>
        {
            new OrganisationSearchByPponResult
            {
                Id = Guid.NewGuid(),
                Name = "Demo Org",
                Type = OrganisationType.Organisation,
                Identifiers = new List<Identifier>
                {
                    new Identifier
                    {
                        Scheme = "GB-PPON",
                        Id = "PGWZ-1758-DEMO",
                        LegalName = "Demo Org"
                    }
                },
                Roles = new List<PartyRole> { PartyRole.Buyer },
                Addresses = new List<OrganisationAddress>()
            }
        };

        int totalCount = 1;
        var searchResponse = (searchResults, totalCount);

        _searchByNameOrPponUseCase.Setup(uc => uc.Execute(It.IsAny<OrganisationSearchByPponQuery>()))
            .ReturnsAsync(searchResponse);

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            Channel.OneLogin, organisationId, OrganisationPersonScope.Admin,
            services => services.AddScoped(_ => _searchByNameOrPponUseCase.Object));

        var response = await factory.CreateClient().GetAsync($"/organisation/search-by-name-or-ppon?searchText=test&limit=10&skip=0&sortOrder={sortOrder}");

        response.StatusCode.Should().Be(expectedStatusCode);

        if (expectedStatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadFromJsonAsync<OrganisationSearchByPponResponse>();
            content.Should().NotBeNull();
            content!.Results.Should().HaveCount(1);
            content.TotalCount.Should().Be(1);

            _searchByNameOrPponUseCase.Verify(uc => uc.Execute(
                It.Is<OrganisationSearchByPponQuery>(q =>
                    q.OrderBy == (string.IsNullOrEmpty(sortOrder) ? "rel" : sortOrder))),
                Times.Once);
        }
    }


    public static Model.Organisation GivenOrganisation(Guid organisationId)
    {
        var command = GivenRegisterOrganisationCommand();
        return new Model.Organisation
        {
            Id = organisationId,
            Name = "TheOrganisation",
            Type = OrganisationType.Organisation,
            Identifier = command.Identifier.AsView(),
            AdditionalIdentifiers = command.AdditionalIdentifiers.AsView(),
            Addresses = command.Addresses.AsView(),
            ContactPoint = command.ContactPoint.AsView(),
            Roles = command.Roles,
            Details = new Details()
        };
    }

    private static RegisterOrganisation GivenRegisterOrganisationCommand()
    {
        return new RegisterOrganisation
        {
            Name = "TheOrganisation",
            Type = OrganisationType.Organisation,
            Identifier = new OrganisationIdentifier
            {
                Scheme = "ISO9001",
                Id = "1",
                LegalName = "OfficialOrganisationName"
            },
            AdditionalIdentifiers = new List<OrganisationIdentifier>
            {
                new OrganisationIdentifier
                {
                    Scheme = "ISO14001",
                    Id = "2",
                    LegalName = "AnotherOrganisationName"
                }
            },
            Addresses = [new OrganisationAddress
            {
                Type = AddressType.Registered,
                StreetAddress = "1234 Example St",
                Locality = "Example City",
                Region = "Test Region",
                PostalCode = "12345",
                CountryName = "Exampleland",
                Country = "AB"
            }],
            ContactPoint = new OrganisationContactPoint
            {
                Name = "Contact Name",
                Email = "contact@example.org",
                Telephone = "123-456-7890",
                Url = "https://example.org/contact"
            },
            Roles = [PartyRole.Tenderer]
        };
    }

    private Model.Organisation SetupRegisterOrganisationUseCaseMock(RegisterOrganisation command)
    {
        var organisation = new Model.Organisation
        {
            Id = Guid.NewGuid(),
            Name = "TheOrganisation",
            Type = OrganisationType.Organisation,
            Identifier = command.Identifier.AsView(),
            AdditionalIdentifiers = command.AdditionalIdentifiers.AsView(),
            Addresses = command.Addresses.AsView(),
            ContactPoint = command.ContactPoint.AsView(),
            Roles = command.Roles,
            Details = new Details()
        };

        _registerOrganisationUseCase.Setup(useCase => useCase.Execute(It.IsAny<RegisterOrganisation>()))
                                    .ReturnsAsync(organisation);

        return organisation;
    }
}
