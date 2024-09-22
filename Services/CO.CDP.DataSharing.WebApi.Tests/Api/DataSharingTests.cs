using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.DataSharing.WebApi.UseCase;
using CO.CDP.TestKit.Mvc;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using System.Net.Http.Json;
using static CO.CDP.Authentication.Constants;
using static System.Net.HttpStatusCode;

namespace CO.CDP.DataSharing.WebApi.Tests.Api;

public class DataSharingTests
{
    private readonly Mock<IUseCase<string, SupplierInformation?>> _getSharedDataUseCase = new();
    private readonly Mock<IUseCase<string, byte[]?>> _getSharedDataPdfUseCase = new();
    private readonly Mock<IUseCase<ShareRequest, ShareReceipt>> _generateShareCodeUseCase = new();
    private readonly Mock<IUseCase<ShareVerificationRequest, ShareVerificationReceipt>> _getShareCodeVerifyUseCase = new();
    private readonly Mock<IUseCase<Guid, List<SharedConsent>?>> _getShareCodesUseCase = new();
    private readonly Mock<IUseCase<(Guid, string), SharedConsentDetails?>> _getShareCodeDetailsUseCase = new();

    [Theory]
    [InlineData(OK, Channel.OrganisationKey)]
    [InlineData(Forbidden, Channel.ServiceKey)]
    [InlineData(Forbidden, Channel.OneLogin)]
    [InlineData(Forbidden, "unknown_channel")]
    public async Task GetSharedData_Authorization_ReturnsExpectedStatusCode(
        HttpStatusCode expectedStatusCode, string channel)
    {
        var shareCode = "valid-share-code";

        _getSharedDataUseCase.Setup(uc => uc.Execute(shareCode))
            .ReturnsAsync(GetSupplierInfo());

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            channel, serviceCollection: s => s.AddScoped(_ => _getSharedDataUseCase.Object));

        var response = await factory.CreateClient().GetAsync($"/share/data/{shareCode}");

        response.StatusCode.Should().Be(expectedStatusCode);
    }

    [Theory]
    [InlineData(OK, Channel.OneLogin)]
    [InlineData(Forbidden, Channel.ServiceKey)]
    [InlineData(Forbidden, Channel.OrganisationKey)]
    [InlineData(Forbidden, "unknown_channel")]
    public async Task GetSharedDataPdf_Authorization_ReturnsExpectedStatusCode(
        HttpStatusCode expectedStatusCode, string channel)
    {
        var shareCode = "valid-share-code";

        _getSharedDataPdfUseCase.Setup(uc => uc.Execute(shareCode)).ReturnsAsync(new byte[1]);

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            channel, serviceCollection: s => s.AddScoped(_ => _getSharedDataPdfUseCase.Object));

        var response = await factory.CreateClient().GetAsync($"/share/data/{shareCode}/pdf");

        response.StatusCode.Should().Be(expectedStatusCode);
    }

    [Theory]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Admin)]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Editor)]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Responder)]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Viewer)]
    [InlineData(Forbidden, Channel.ServiceKey)]
    [InlineData(Forbidden, Channel.OrganisationKey)]
    [InlineData(Forbidden, "unknown_channel")]
    public async Task CreateSharedData_Authorization_ReturnsExpectedStatusCode(
        HttpStatusCode expectedStatusCode, string channel, string? scope = null)
    {
        var organisationId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var shareRequest = new ShareRequest { FormId = formId, OrganisationId = organisationId };

        _generateShareCodeUseCase.Setup(uc => uc.Execute(shareRequest))
            .ReturnsAsync(new ShareReceipt { FormId = formId, ShareCode = "new_code" });

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            channel, organisationId, scope,
            services => services.AddScoped(_ => _generateShareCodeUseCase.Object));

        var response = await factory.CreateClient().PostAsJsonAsync("/share/data", shareRequest);

        response.StatusCode.Should().Be(expectedStatusCode);
    }

    [Theory]
    [InlineData(OK, Channel.OrganisationKey)]
    [InlineData(Forbidden, Channel.ServiceKey)]
    [InlineData(Forbidden, Channel.OneLogin)]
    [InlineData(Forbidden, "unknown_channel")]
    public async Task VerifySharedData_Authorization_ReturnsExpectedStatusCode(
        HttpStatusCode expectedStatusCode, string channel)
    {
        var shareCode = "new_code";
        var formVersionId = "v.0";
        var shareRequest = new ShareVerificationRequest { ShareCode = shareCode, FormVersionId = formVersionId };

        _getShareCodeVerifyUseCase.Setup(uc => uc.Execute(shareRequest))
            .ReturnsAsync(new ShareVerificationReceipt { ShareCode = shareCode, FormVersionId = formVersionId, IsLatest = true });

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            channel, serviceCollection: s => s.AddScoped(_ => _getShareCodeVerifyUseCase.Object));

        var response = await factory.CreateClient().PostAsJsonAsync("/share/data/verify", shareRequest);

        response.StatusCode.Should().Be(expectedStatusCode);
    }

    [Theory]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Admin)]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Editor)]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Viewer)]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Responder)]
    [InlineData(Forbidden, Channel.ServiceKey)]
    [InlineData(Forbidden, Channel.OrganisationKey)]
    [InlineData(Forbidden, "unknown_channel")]
    public async Task GetShareCodeList_Authorization_ReturnsExpectedStatusCode(
        HttpStatusCode expectedStatusCode, string channel, string? scope = null)
    {
        var organisationId = Guid.NewGuid();

        _getShareCodesUseCase.Setup(uc => uc.Execute(organisationId))
            .ReturnsAsync([new SharedConsent()]);

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            channel, organisationId, scope,
            services => services.AddScoped(_ => _getShareCodesUseCase.Object));

        var response = await factory.CreateClient().GetAsync($"/share/organisations/{organisationId}/codes");

        response.StatusCode.Should().Be(expectedStatusCode);
    }

    [Theory]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Admin)]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Editor)]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Viewer)]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Responder)]
    [InlineData(Forbidden, Channel.ServiceKey)]
    [InlineData(Forbidden, Channel.OrganisationKey)]
    [InlineData(Forbidden, "unknown_channel")]
    public async Task GetShareCodeDetails_Authorization_ReturnsExpectedStatusCode(
        HttpStatusCode expectedStatusCode, string channel, string? scope = null)
    {
        var organisationId = Guid.NewGuid();
        var shareCode = "new_code";
        var command = (organisationId, shareCode);

        _getShareCodeDetailsUseCase.Setup(uc => uc.Execute(command))
            .ReturnsAsync(new SharedConsentDetails { ShareCode = shareCode });

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            channel, organisationId, scope,
            services => services.AddScoped(_ => _getShareCodeDetailsUseCase.Object));

        var response = await factory.CreateClient().GetAsync($"/share/organisations/{organisationId}/codes/{shareCode}");

        response.StatusCode.Should().Be(expectedStatusCode);
    }

    private static SupplierInformation GetSupplierInfo()
    {
        return new SupplierInformation
        {
            Id = Guid.NewGuid(),
            Name = "si",
            AssociatedPersons = [],
            AdditionalParties = [],
            AdditionalEntities = [],
            Identifier = new OrganisationInformation.Identifier { Scheme = "fake", LegalName = "test" },
            AdditionalIdentifiers = [],
            Address = new OrganisationInformation.Address
            {
                StreetAddress = "1 st",
                Locality = "very local",
                PostalCode = "WS1",
                Country = "GB",
                CountryName = "UK",
                Type = OrganisationInformation.AddressType.Registered
            },
            ContactPoint = new OrganisationInformation.ContactPoint(),
            Roles = [],
            Details = new Details(),
            SupplierInformationData = new SupplierInformationData
            {
                Form = new Form
                {
                    FormId = Guid.NewGuid(),
                    Name = "f1",
                    FormVersionId = "v.0",
                    OrganisationId = Guid.NewGuid(),
                    IsRequired = false,
                    ShareCode = "new_code",
                    SubmissionState = FormSubmissionState.Submitted,
                    SubmittedAt = DateTime.Now
                },
                Questions = [],
                AnswerSets = []
            }
        };
    }
}