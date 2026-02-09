using CO.CDP.ApplicationRegistry.Core.Exceptions;
using CO.CDP.ApplicationRegistry.Core.Interfaces;
using CO.CDP.ApplicationRegistry.Core.Models;
using CO.CDP.Logging;
using CO.CDP.Person.WebApiClient;
using Microsoft.Extensions.Logging;

namespace CO.CDP.ApplicationRegistry.Infrastructure.Services;

/// <summary>
/// Service for looking up person details from the CDP Person API.
/// </summary>
public class PersonLookupService : IPersonLookupService
{
    private readonly IPersonClient _personClient;
    private readonly SanitisingLogger<PersonLookupService> _logger;

    public PersonLookupService(
        IPersonClient personClient,
        SanitisingLogger<PersonLookupService> logger)
    {
        _personClient = personClient;
        _logger = logger;
    }

    public async Task<PersonDetails?> GetPersonDetailsAsync(string userPrincipalId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userPrincipalId))
        {
            throw new ArgumentException("User principal ID cannot be null or empty.", nameof(userPrincipalId));
        }

        try
        {
            _logger.LogInformation(
                "Looking up person details for UserPrincipalId: {UserPrincipalId}",
                userPrincipalId);

            var person = await _personClient.LookupPersonAsync(userPrincipalId, null, cancellationToken);

            if (person == null)
            {
                _logger.LogWarning(
                    "Person not found for UserPrincipalId: {UserPrincipalId}",
                    userPrincipalId);
                return null;
            }

            _logger.LogInformation(
                "Successfully retrieved person details for UserPrincipalId: {UserPrincipalId}",
                userPrincipalId);

            return new PersonDetails
            {
                FirstName = person.FirstName,
                LastName = person.LastName,
                Email = person.Email,
                CdpPersonId = person.Id
            };
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            _logger.LogWarning(
                ex,
                "Person not found (404) for UserPrincipalId: {UserPrincipalId}",
                userPrincipalId);
            return null;
        }
        catch (ApiException ex)
        {
            _logger.LogError(
                ex,
                "API error occurred while looking up person for UserPrincipalId: {UserPrincipalId}. Status: {StatusCode}",
                userPrincipalId,
                ex.StatusCode);
            throw new PersonLookupException(
                $"Failed to lookup person details for UserPrincipalId: {userPrincipalId}. Status: {ex.StatusCode}",
                ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error occurred while looking up person for UserPrincipalId: {UserPrincipalId}",
                userPrincipalId);
            throw new PersonLookupException(
                $"Failed to lookup person details for UserPrincipalId: {userPrincipalId}",
                ex);
        }
    }
}
