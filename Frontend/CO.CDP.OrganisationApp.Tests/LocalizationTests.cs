using System.Net;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.TestKit.Mvc;
using FluentAssertions;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

namespace CO.CDP.OrganisationApp.Tests;
public class LocalizationTests
{
    private HttpClient BuildHttpClient()
    {
        Mock<ISession> session = new();
        Guid personId = new("5b0d3aa8-94cd-4ede-ba03-546937035690");

        var person = new Person.WebApiClient.Person("a@b.com", "First name", personId, "Last name", null);

        session
            .Setup(s => s.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails() { Email = "a@b.com", UserUrn = "urn", PersonId = person.Id });

        session
            .Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey))
            .Returns(new RegistrationDetails() { OrganisationType = OrganisationType.Supplier, OrganisationScheme = "Whatever" });

        var antiforgery = new Mock<IAntiforgery>();

        antiforgery.Setup(a => a.ValidateRequestAsync(It.IsAny<HttpContext>()))
                       .Returns(Task.CompletedTask);

        antiforgery.Setup(a => a.GetAndStoreTokens(It.IsAny<HttpContext>()))
               .Returns(new AntiforgeryTokenSet(
                   "fakeRequestToken",
                   "fakeCookieToken",
                   "fakeFormFieldName",
                   "fakeHeaderName"));

        var factory = new TestWebApplicationFactory<Program>(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(session.Object);
                services.AddSingleton(antiforgery.Object);
                services.AddTransient<IAuthenticationSchemeProvider, FakeSchemeProvider>();
                services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options => {
                    options.ClientId = "123";
                    options.Authority = "https://whatever";
                });
                services.AddDataProtection().DisableAutomaticKeyGeneration();
            });
        });

        return factory.CreateClient();
    }

    [Fact]
    public async Task OrganisationNamePage_DisplaysCorrectly_WhenLanguageIsDefaulted()
    {
        var httpClient = BuildHttpClient();

        var request = new HttpRequestMessage(HttpMethod.Get, "/registration/organisation-name");

        var response = await httpClient.SendAsync(request);

        var responseBody = await response.Content.ReadAsStringAsync();

        responseBody.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        responseBody.Should().Contain("Enter the organisation&#x27;s name");
    }

    [Fact]
    public async Task OrganisationNamePage_DisplaysCorrectly_WhenLanguageIsEnglish()
    {
        var httpClient = BuildHttpClient();

        var request = new HttpRequestMessage(HttpMethod.Get, "/registration/organisation-name");

        var cultureCookieValue = CookieRequestCultureProvider.MakeCookieValue(new RequestCulture("en"));
        request.Headers.Add("Cookie", $"{CookieRequestCultureProvider.DefaultCookieName}={cultureCookieValue}");

        var response = await httpClient.SendAsync(request);

        var responseBody = await response.Content.ReadAsStringAsync();

        responseBody.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        responseBody.Should().Contain("Enter the organisation&#x27;s name");
    }

    [Fact]
    public async Task OrganisationNamePage_DisplaysCorrectly_WhenLanguageIsWelsh()
    {
        var httpClient = BuildHttpClient();

        var request = new HttpRequestMessage(HttpMethod.Get, "/registration/organisation-name");

        var cultureCookieValue = CookieRequestCultureProvider.MakeCookieValue(new RequestCulture("cy"));
        request.Headers.Add("Cookie", $"{CookieRequestCultureProvider.DefaultCookieName}={cultureCookieValue}");

        var response = await httpClient.SendAsync(request);

        var responseBody = await response.Content.ReadAsStringAsync();

        responseBody.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        responseBody.Should().Contain("Rhowch enw&#x2019;r sefydliad");
    }

    [Fact]
    public async Task OrganisationNamePage_DisplaysErrorMessageCorrectly_WhenLanguageIsEnglish()
    {
        var httpClient = BuildHttpClient();

        var request = new HttpRequestMessage(HttpMethod.Post, "/registration/organisation-name")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string> { { "OrganisationName", "" } })
        };

        var response = await httpClient.SendAsync(request);

        var responseBody = await response.Content.ReadAsStringAsync();

        responseBody.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        responseBody.Should().Contain("<span class=\"field-validation-error\" data-valmsg-for=\"OrganisationName\" data-valmsg-replace=\"true\">Enter the organisation&#x27;s name</span>");
    }

    [Fact]
    public async Task OrganisationNamePage_DisplaysErrorMessageCorrectly_WhenLanguageIsWelsh()
    {
        var httpClient = BuildHttpClient();

        var request = new HttpRequestMessage(HttpMethod.Post, "/registration/organisation-name")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string> { { "OrganisationName", "" } })
        };

        var cultureCookieValue = CookieRequestCultureProvider.MakeCookieValue(new RequestCulture("cy"));
        request.Headers.Add("Cookie", $"{CookieRequestCultureProvider.DefaultCookieName}={cultureCookieValue}");

        var response = await httpClient.SendAsync(request);

        var responseBody = await response.Content.ReadAsStringAsync();

        responseBody.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        responseBody.Should().Contain("<span class=\"field-validation-error\" data-valmsg-for=\"OrganisationName\" data-valmsg-replace=\"true\">Rhowch enw&#x2019;r sefydliad</span>");
    }
}
