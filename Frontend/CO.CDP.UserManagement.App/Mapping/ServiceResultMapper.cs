using CO.CDP.Functional;
using CO.CDP.UserManagement.WebApiClient;

namespace CO.CDP.UserManagement.App.Services;

public static class ServiceResultMapper
{
    public static Result<ServiceFailure, ServiceOutcome> FromApiException(ApiException exception)
    {
        return exception.StatusCode == 404
            ? Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.NotFound)
            : Result<ServiceFailure, ServiceOutcome>.Failure(ServiceFailure.Unexpected);
    }
}
