using CO.CDP.ApplicationRegistry.Core.Entities;
using CO.CDP.ApplicationRegistry.Shared.Responses;

namespace CO.CDP.ApplicationRegistry.Api.Models;

/// <summary>
/// Extension methods for application mapping.
/// </summary>
public static class ApplicationMappingExtensions
{
    public static ApplicationResponse ToResponse(this Application application)
    {
        return new ApplicationResponse
        {
            Id = application.Id,
            Name = application.Name,
            ClientId = application.ClientId,
            Description = application.Description,
            Category = application.Category,
            IsActive = application.IsActive,
            CreatedAt = application.CreatedAt
        };
    }

    public static ApplicationSummaryResponse ToSummaryResponse(this Application application)
    {
        return new ApplicationSummaryResponse
        {
            Id = application.Id,
            Name = application.Name,
            ClientId = application.ClientId,
            IsActive = application.IsActive
        };
    }
}