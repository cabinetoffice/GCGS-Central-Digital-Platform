using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.ThirdPartyApiClients.CompaniesHouse;
using CO.CDP.Person.WebApiClient;
using CO.CDP.Tenant.WebApiClient;
using FluentAssertions;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using System.Text.RegularExpressions;

namespace CO.CDP.OrganisationApp.Tests;
public class LocalizationTests
{
    private static readonly Mock<OrganisationClient> organisationClient = new Mock<OrganisationClient>("https://whatever", new HttpClient());
    private static readonly Mock<ISession> _mockSession = new Mock<ISession>();
    private static Guid personId = new Guid("5b0d3aa8-94cd-4ede-ba03-546937035690");
    private static readonly Mock<ICompaniesHouseApi> companiesHouseApiMock = new Mock<ICompaniesHouseApi>();

    public HttpClient BuildHttpClient()
    {
        var services = new ServiceCollection();

        var person = new Person.WebApiClient.Person("a@b.com", "First name", personId, "Last name", null);

        _mockSession
            .Setup(s => s.Get<Models.UserDetails>(Session.UserDetailsKey))
            .Returns(new Models.UserDetails() { Email = "a@b.com", UserUrn = "urn", PersonId = person.Id });

        _mockSession
            .Setup(s => s.Get<Models.RegistrationDetails>(Session.RegistrationDetailsKey))
            .Returns(new Models.RegistrationDetails() { OrganisationType = OrganisationType.Supplier, OrganisationScheme = "Whatever" });

        services.AddSingleton(_mockSession.Object);

        var antiforgeryMock = new Mock<IAntiforgery>();

        antiforgeryMock.Setup(a => a.ValidateRequestAsync(It.IsAny<HttpContext>()))
                       .Returns(Task.CompletedTask);

        antiforgeryMock.Setup(a => a.GetAndStoreTokens(It.IsAny<HttpContext>()))
               .Returns(new AntiforgeryTokenSet(
                   "fakeRequestToken",
                   "fakeCookieToken", 
                   "fakeFormFieldName",
                   "fakeHeaderName")); 

        services.AddSingleton(antiforgeryMock.Object);

        services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
        .AddScheme<AuthenticationSchemeOptions, FakeCookieAuthHandler>(CookieAuthenticationDefaults.AuthenticationScheme, options => { });

        services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options => {
            options.ClientId = "123";
            options.Authority = "https://whatever";
        });

        var factory = new CustomisableWebApplicationFactory<Program>(services);

        return factory.CreateClient();
    }

    [Fact]
    public async Task OrganisationNamePage_DisplaysCorrectly_WhenLanguageIsDefaulted()
    {
        var _httpClient = BuildHttpClient();

        var request = new HttpRequestMessage(HttpMethod.Get, "/registration/organisation-name");

        var response = await _httpClient.SendAsync(request);

        var responseBody = await response.Content.ReadAsStringAsync();

        responseBody.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        responseBody.Should().Contain("Enter the organisation&#x27;s name");
    }

    [Fact]
    public async Task OrganisationNamePage_DisplaysCorrectly_WhenLanguageIsEnglish()
    {
        var _httpClient = BuildHttpClient();

        var request = new HttpRequestMessage(HttpMethod.Get, "/registration/organisation-name");

        var cultureCookieValue = CookieRequestCultureProvider.MakeCookieValue(new RequestCulture("en"));
        request.Headers.Add("Cookie", $"{CookieRequestCultureProvider.DefaultCookieName}={cultureCookieValue}");

        var response = await _httpClient.SendAsync(request);

        var responseBody = await response.Content.ReadAsStringAsync();

        responseBody.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        responseBody.Should().Contain("Enter the organisation&#x27;s name");
    }

    [Fact]
    public async Task OrganisationNamePage_DisplaysCorrectly_WhenLanguageIsWelsh()
    {
        var _httpClient = BuildHttpClient();

        var request = new HttpRequestMessage(HttpMethod.Get, "/registration/organisation-name");

        var cultureCookieValue = CookieRequestCultureProvider.MakeCookieValue(new RequestCulture("cy"));
        request.Headers.Add("Cookie", $"{CookieRequestCultureProvider.DefaultCookieName}={cultureCookieValue}");

        var response = await _httpClient.SendAsync(request);

        var responseBody = await response.Content.ReadAsStringAsync();

        responseBody.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        responseBody.Should().Contain("Rhowch enw&#x2019;r sefydliad");
    }

    [Fact]
    public async Task OrganisationNamePage_DisplaysErrorMessageCorrectly_WhenLanguageIsEnglish()
    {
        var _httpClient = BuildHttpClient();

        var request = new HttpRequestMessage(HttpMethod.Post, "/registration/organisation-name")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string> { { "OrganisationName", "" } })
        };

        var response = await _httpClient.SendAsync(request);

        var responseBody = await response.Content.ReadAsStringAsync();

        responseBody.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        responseBody.Should().Contain("<span class=\"field-validation-error\" data-valmsg-for=\"OrganisationName\" data-valmsg-replace=\"true\">Enter the organisation&#x27;s name</span>");
    }

    [Fact]
    public async Task OrganisationNamePage_DisplaysErrorMessageCorrectly_WhenLanguageIsWelsh()
    {
        var _httpClient = BuildHttpClient();

        var request = new HttpRequestMessage(HttpMethod.Post, "/registration/organisation-name")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string> { { "OrganisationName", "" } })
        };

        var cultureCookieValue = CookieRequestCultureProvider.MakeCookieValue(new RequestCulture("cy"));
        request.Headers.Add("Cookie", $"{CookieRequestCultureProvider.DefaultCookieName}={cultureCookieValue}");

        var response = await _httpClient.SendAsync(request);

        var responseBody = await response.Content.ReadAsStringAsync();

        responseBody.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        responseBody.Should().Contain("<span class=\"field-validation-error\" data-valmsg-for=\"OrganisationName\" data-valmsg-replace=\"true\">Rhowch enw&#x2019;r sefydliad</span>");
    }
}
