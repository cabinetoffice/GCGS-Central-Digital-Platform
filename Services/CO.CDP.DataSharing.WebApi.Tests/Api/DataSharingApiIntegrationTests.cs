using CO.CDP.DataSharing.WebApiClient;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.OrganisationInformation.Persistence.Forms;
using CO.CDP.OrganisationInformation.Persistence.Tests;
using CO.CDP.TestKit.Mvc;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit.Abstractions;

namespace CO.CDP.DataSharing.WebApi.Tests.Api;

public class DataSharingApiIntegrationTests: IClassFixture<OrganisationInformationPostgreSqlFixture>
{
    private readonly HttpClient _httpClient;
    private readonly OrganisationInformationPostgreSqlFixture _postgreSql;

    public DataSharingApiIntegrationTests(ITestOutputHelper testOutputHelper, OrganisationInformationPostgreSqlFixture postgreSql)
    {
        TestWebApplicationFactory<Program> _factory = new(builder =>
        {
            builder.ConfigureFakePolicyEvaluator();
            builder.ConfigureLogging(testOutputHelper);

            builder.ConfigureServices((_, services) =>
            {
                services.RemoveAll<OrganisationInformationContext>();
                services.AddScoped(_ => postgreSql.OrganisationInformationContext());
            });
        });

        _httpClient = _factory.CreateClient();
        _postgreSql = postgreSql;
    }

    [Fact]
    public async Task TempGetShareCodesWorking()
    {
        var organisationId = Guid.NewGuid();
        using (var context = _postgreSql.OrganisationInformationContext())
        {
            var organisation = new Organisation
            {
                Guid = organisationId,
                Name = "Test org",
                Type = OrganisationInformation.OrganisationType.Organisation,
                Tenant = new Tenant
                {
                    Guid = Guid.NewGuid(),
                    Name = "Test org",
                },
                Identifiers = new List<Organisation.Identifier>
                {
                    new Organisation.Identifier
                    {
                        IdentifierId = "1234567",
                        Scheme = "Whatever",
                        LegalName = "New Org Legal Name",
                        Primary = true
                    }
                },
                Addresses = new List<Organisation.OrganisationAddress>
                {
                    new Organisation.OrganisationAddress
                    {
                        Type = OrganisationInformation.AddressType.Registered,
                        Address = new OrganisationInformation.Persistence.Address
                        {
                            StreetAddress = "1234 New St",
                            Locality = "New City",
                            Region = "W.Yorkshire",
                            PostalCode = "123456",
                            CountryName = "Newland",
                            Country = "GB"
                        }
                    }
                },
                ContactPoints = new List<Organisation.ContactPoint>
                {
                    new Organisation.ContactPoint
                    {
                        Name = "Main Contact",
                        Email = "foo@bar.com"
                    }
                }
            };

            context.Organisations.Add(organisation);
            context.SaveChanges();

            var form = new OrganisationInformation.Persistence.Forms.Form
            {
                Guid = Guid.NewGuid(),
                Name = "Test form",
                Version = "1",
                IsRequired = true,
                Scope = FormScope.SupplierInformation,
                Sections = []
            };

            context.Forms.Add(form);
            context.SaveChanges();

            context.SharedConsents.Add(new OrganisationInformation.Persistence.Forms.SharedConsent
            {
                Guid =  new Guid(),
                OrganisationId = organisation.Id,
                Organisation = organisation,
                FormId = 1,
                Form = form,
                FormVersionId = "1",
                SubmissionState = SubmissionState.Submitted,
                ShareCode = "ABC123",
                SubmittedAt = DateTimeOffset.Now,
            });

            context.SaveChanges();
        }

        IDataSharingClient client = new DataSharingClient("https://localhost", _httpClient);

        var response = await client.GetSharedDataAsync("ABC123");
        response.Id.Should().Be(organisationId);
    }
}


//Func<Task> act = async () => await client.GetSharedDataAsync("ABC123");

//var exception = await act.Should().ThrowAsync<ApiException<ProblemDetails>>();
//exception.Which.StatusCode.Should().Be(404);
//exception.Which.Result.Detail.Should().Contain("Share code not found");