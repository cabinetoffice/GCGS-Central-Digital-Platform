// using System.Net;
// using System.Threading.Tasks;
// using Xunit;
// using Microsoft.AspNetCore.Mvc.Testing;
// using System.Text.Json;
// using CO.CDP.Common.Auth;
// using CO.CDP.Login.WebApi;

// namespace CO.CDP.Login.WebApi.Tests;

// public class LoginTests : IClassFixture<CustomWebApplicationFactory>
// {
//     private readonly HttpClient _client;

//     public ApiTests(CustomWebApplicationFactory factory)
//     {
//         _client = factory.CreateClient();
//     }

//     [Fact]
//     public async Task GetFakeOneLogin_ReturnsExpectedResponse()
//     {
//         // Arrange
//         var client = _factory.CreateClient();

//         // Act
//         var response = await client.GetAsync("/fake-onelogin/");

//         // Assert
//         response.EnsureSuccessStatusCode(); // Status Code 200-299
//         Assert.Equal(HttpStatusCode.OK, response.StatusCode);

//         var jsonResponse = await response.Content.ReadAsStringAsync();
//         var oneLoginResponse = JsonSerializer.Deserialize<OneLoginResponse>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

//         Assert.NotNull(oneLoginResponse);
//         Assert.Equal("urn:fdc:gov.uk:2022:56P4CMsGh_-2sVIB2nsNU7mcLZYhYw=", oneLoginResponse.Sub);
//         Assert.Equal("test@example.com", oneLoginResponse.Email);
//         Assert.True(oneLoginResponse.EmailVerified);
//         Assert.Equal("01406946277", oneLoginResponse.PhoneNumber);
//         Assert.True(oneLoginResponse.PhoneNumberVerified);
//         Assert.Equal(1311280970L, oneLoginResponse.UpdatedAt);
//     }
//     [Fact]
//     public void Test1()
//     {
//         Assert.True(true);
//     }
// }
