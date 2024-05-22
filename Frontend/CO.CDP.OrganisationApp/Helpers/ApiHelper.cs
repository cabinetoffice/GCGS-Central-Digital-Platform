using CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Helpers;

public static class ApiHelper
{
    public static async Task<T> CallApiAsync<T>(Func<Task<T>> apiCall, string errorMessage)
    {
        try
        {
            return await apiCall();
        }
        catch (ApiException apiEx) when (apiEx.StatusCode == 400)
        {
            throw new ApiException(errorMessage + " Invalid data provided.", apiEx.StatusCode, apiEx.Response, apiEx.Headers, apiEx);
        }
        catch (ApiException apiEx) when (apiEx.StatusCode == 401)
        {
            throw new ApiException(errorMessage + " Unauthorized access.", apiEx.StatusCode, apiEx.Response, apiEx.Headers, apiEx);
        }
        catch (ApiException apiEx) when (apiEx.StatusCode == 404)
        {
            throw new ApiException(errorMessage + " Resource not found.", apiEx.StatusCode, apiEx.Response, apiEx.Headers, apiEx);
        }
        catch (Exception ex)
        {
            throw new Exception(errorMessage + " An unexpected error occurred.", ex);
        }
    }
}