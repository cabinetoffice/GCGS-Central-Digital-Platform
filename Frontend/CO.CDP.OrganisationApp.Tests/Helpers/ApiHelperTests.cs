using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Helpers;
using FluentAssertions;

namespace CO.CDP.OrganisationApp.Tests.Helpers;
public class ApiHelperTests
{
    [Fact]
    public async Task ShouldReturn_Successful_Status_WhenApiCallIsSuccessful()
    {
        var expectedResult = "Success";
        Func<Task<string>> apiCall = () => Task.FromResult(expectedResult);

        var result = await ApiHelper.CallApiAsync(apiCall, "Test");

        result.Should().Be(expectedResult);
    }

    [Fact]
    public async Task ShouldThrowApiException_WithBadRequestMessage_WhenApiExceptionWithStatusCode400Occurs()
    {
        Func<Task<string>> apiCall = () => throw new ApiException("Bad Request", 400, null, null, null);

        Func<Task> action = async () => await ApiHelper.CallApiAsync(apiCall, "Test");

        var exception = await action.Should().ThrowAsync<ApiException>();
        exception.Which.StatusCode.Should().Be(400);
        exception.Which.Message.Should().Contain("Test Invalid data provided.");
    }

    [Fact]
    public async Task ShouldThrowApiException_WithUnauthorizedMessage_WhenApiExceptionWithStatusCode401Occurs()
    {
        Func<Task<string>> apiCall = () => throw new ApiException("Unauthorized", 401, null, null, null);

        Func<Task> action = async () => await ApiHelper.CallApiAsync(apiCall, "Test");

        var exception = await action.Should().ThrowAsync<ApiException>();
        exception.Which.StatusCode.Should().Be(401);
        exception.Which.Message.Should().Contain("Test Unauthorized access.");
    }

    [Fact]
    public async Task ShouldThrowApiException_WithNotFoundMessage_WhenApiExceptionWithStatusCode404Occurs()
    {
        Func<Task<string>> apiCall = () => throw new ApiException("Not Found", 404, null, null, null);

        Func<Task> action = async () => await ApiHelper.CallApiAsync(apiCall, "Test");

        var exception = await action.Should().ThrowAsync<ApiException>();
        exception.Which.StatusCode.Should().Be(404);
        exception.Which.Message.Should().Contain("Test Resource not found.");
    }

    [Fact]
    public async Task ShouldThrowException_WithUnexpectedErrorMessage_WhenGeneralExceptionOccurs()
    {
        Func<Task<string>> apiCall = () => throw new Exception("General Error");

        Func<Task> action = async () => await ApiHelper.CallApiAsync(apiCall, "Test");

        var exception = await action.Should().ThrowAsync<Exception>();
        exception.Which.Message.Should().Contain("Test An unexpected error occurred.");
    }
}