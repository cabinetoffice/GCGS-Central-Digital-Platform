using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Core.Models;
using CO.CDP.Person.WebApiClient;
using Microsoft.Extensions.Logging;

namespace CO.CDP.UserManagement.Infrastructure.Services;

/// <summary>
/// Service for looking up person details from the CDP Person API.
/// </summary>
public class PersonLookupService : IPersonLookupService
{
    private readonly IPersonClient _personClient;
    private readonly ILogger<PersonLookupService> _logger;

    public PersonLookupService(
        IPersonClient personClient,
        ILogger<PersonLookupService> logger)
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

    public async Task<IReadOnlyDictionary<Guid, PersonDetails>> GetPersonDetailsByIdsAsync(
        IEnumerable<Guid> cdpPersonIds,
        CancellationToken cancellationToken = default)
    {
        if (cdpPersonIds == null)
        {
            throw new ArgumentNullException(nameof(cdpPersonIds));
        }

        var personIdList = cdpPersonIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (personIdList.Count == 0)
        {
            return new Dictionary<Guid, PersonDetails>();
        }

        try
        {
            _logger.LogInformation("Looking up {PersonCount} person IDs", personIdList.Count);

            var personIdStrings = personIdList.Select(id => id.ToString()).ToList();
            var personsById = await _personClient.BulkLookupPersonAsync(
                new BulkLookupPerson(personIdStrings),
                cancellationToken);

            var result = new Dictionary<Guid, PersonDetails>();
            foreach (var (key, person) in personsById)
            {
                if (!Guid.TryParse(key, out var id))
                {
                    _logger.LogWarning("Skipping person lookup entry with non-GUID key {PersonKey}", key);
                    continue;
                }

                result[id] = new PersonDetails
                {
                    FirstName = person.FirstName,
                    LastName = person.LastName,
                    Email = person.Email,
                    CdpPersonId = person.Id
                };
            }

            var missingPersonIds = personIdList.Where(id => !result.ContainsKey(id)).ToList();
            if (missingPersonIds.Count > 0)
            {
                _logger.LogWarning("Person lookup missing {MissingCount} IDs: {MissingPersonIds}",
                    missingPersonIds.Count, missingPersonIds);
            }

            return result;
        }
        catch (ApiException ex)
        {
            _logger.LogError(
                ex,
                "API error occurred while looking up persons. Status: {StatusCode}",
                ex.StatusCode);
            throw new PersonLookupException(
                $"Failed to lookup person details by ID. Status: {ex.StatusCode}",
                ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error occurred while looking up persons");
            throw new PersonLookupException("Failed to lookup person details by ID.", ex);
        }
    }
}
