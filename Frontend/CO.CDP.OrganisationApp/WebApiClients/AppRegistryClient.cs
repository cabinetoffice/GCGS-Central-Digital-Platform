using System.Net.Http.Json;
using System.Text.Json;

namespace CO.CDP.OrganisationApp.WebApiClients;

/// <summary>
/// Concrete HTTP client implementation for the ApplicationRegistry API.
/// All endpoints are backed by MongoDB (AppRegistry persistence layer).
/// Uses the same HTTP client as <c>IOrganisationClient</c> (OrganisationService base URL
/// with bearer token handler) since the AppRegistry endpoints live on the same service.
/// </summary>
public class AppRegistryClient(string baseUrl, HttpClient httpClient) : IAppRegistryClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    // ── Applications ────────────────────────────────────────────────────────

    public async Task<ICollection<AppRegistryApplicationDto>> GetOrganisationApplicationsAsync(Guid orgId)
    {
        var response = await httpClient.GetAsync($"{baseUrl}/api/organisations/{orgId}/applications");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<AppRegistryApplicationDto>>(JsonOptions)
               ?? [];
    }

    public async Task<AppRegistryApplicationDto?> GetApplicationAsync(Guid appId)
    {
        var response = await httpClient.GetAsync($"{baseUrl}/api/applications/{appId}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AppRegistryApplicationDto>(JsonOptions);
    }

    public async Task<ICollection<AppRegistryRoleDto>> GetApplicationRolesAsync(Guid appId)
    {
        var response = await httpClient.GetAsync($"{baseUrl}/api/applications/{appId}/roles");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<AppRegistryRoleDto>>(JsonOptions)
               ?? [];
    }

    // ── User Assignments ────────────────────────────────────────────────────

    public async Task<ICollection<AppRegistryUserAssignmentDto>> GetUserAssignmentsAsync(Guid orgId, Guid appId)
    {
        var response = await httpClient.GetAsync(
            $"{baseUrl}/api/organisations/{orgId}/applications/{appId}/users");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<AppRegistryUserAssignmentDto>>(JsonOptions)
               ?? [];
    }

    public async Task<AppRegistryUserAssignmentDto> AssignUserAsync(
        Guid orgId, Guid appId, CreateAppRegistryUserAssignment cmd)
    {
        var response = await httpClient.PostAsJsonAsync(
            $"{baseUrl}/api/organisations/{orgId}/applications/{appId}/users", cmd, JsonOptions);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AppRegistryUserAssignmentDto>(JsonOptions))!;
    }

    public async Task UpdateUserRolesAsync(
        Guid orgId, Guid appId, string userId, UpdateAppRegistryUserAssignment cmd)
    {
        var response = await httpClient.PutAsJsonAsync(
            $"{baseUrl}/api/organisations/{orgId}/applications/{appId}/users/{Uri.EscapeDataString(userId)}",
            cmd, JsonOptions);
        response.EnsureSuccessStatusCode();
    }

    public async Task RevokeUserAsync(Guid orgId, Guid appId, string userId)
    {
        var response = await httpClient.DeleteAsync(
            $"{baseUrl}/api/organisations/{orgId}/applications/{appId}/users/{Uri.EscapeDataString(userId)}");
        response.EnsureSuccessStatusCode();
    }

    // ── Organisation Members ────────────────────────────────────────────────

    public async Task<ICollection<AppRegistryMemberDto>> GetOrganisationMembersAsync(Guid orgId)
    {
        var response = await httpClient.GetAsync($"{baseUrl}/api/organisations/{orgId}/members");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<AppRegistryMemberDto>>(JsonOptions)
               ?? [];
    }
}
