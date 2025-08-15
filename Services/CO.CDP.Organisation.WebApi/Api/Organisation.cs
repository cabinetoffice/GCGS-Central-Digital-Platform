using CO.CDP.Authentication;
using CO.CDP.Authentication.Authorization;
using CO.CDP.Functional;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation;
using CO.CDP.Swashbuckle.Filter;
using CO.CDP.Swashbuckle.Security;
using CO.CDP.Swashbuckle.SwaggerGen;
using CO.CDP.WebApi.Foundation;
using DotSwashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Address = CO.CDP.OrganisationInformation.Address;
using ConnectedEntity = CO.CDP.Organisation.WebApi.Model.ConnectedEntity;
using ConnectedEntityLookup = CO.CDP.Organisation.WebApi.Model.ConnectedEntityLookup;
using Person = CO.CDP.Organisation.WebApi.Model.Person;

namespace CO.CDP.Organisation.WebApi.Api;

public static class EndpointExtensions
{
    public static void UseGlobalEndpoints(this WebApplication app)
    {
        app.MapGet("/announcements",
            [OrganisationAuthorize([AuthenticationChannel.OneLogin])]
            async (
                [FromQuery] string page,
                IUseCase<GetAnnouncementQuery, IEnumerable<Announcement>> useCase) =>
            {
                var results = await useCase.Execute(new GetAnnouncementQuery
                {
                    Page = page
                });

                return Results.Ok(results);
            })
        .Produces<IEnumerable<Announcement>>(StatusCodes.Status200OK, "application/json")
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
        .WithOpenApi(operation =>
        {
            operation.OperationId = "GetAnnouncements";
            operation.Description = "Gets all active announcements matching filters.";
            operation.Summary = "Get active announcements.";
            operation.Responses["200"].Description = "Announcements retrieved.";
            operation.Responses["401"].Description = "Valid authentication credentials are missing.";
            operation.Responses["404"].Description = "Announcements not found.";
            operation.Responses["500"].Description = "Internal server error.";
            return operation;
        });
    }

    public static void UseOrganisationEndpoints(this WebApplication app)
    {
        app.MapGet("/organisations",
            [OrganisationAuthorize([AuthenticationChannel.OneLogin], personScopes: [Constants.PersonScope.SupportAdmin])]
        async (
            [FromQuery] string? role,
            [FromQuery] string? pendingRole,
            [FromQuery] string? searchText,
            [FromQuery] int limit,
            [FromQuery] int skip,
            IUseCase<PaginatedOrganisationQuery, Tuple<IEnumerable<OrganisationDto>, int>> useCase) =>
                {
                    return await useCase.Execute(new PaginatedOrganisationQuery(limit, skip, role, pendingRole, searchText))
                        .AndThen(organisations => Results.Ok(organisations));
                })
            .Produces<Tuple<IEnumerable<OrganisationDto>, int>> (StatusCodes.Status200OK, "application/json")
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "GetAllOrganisations";
                operation.Description = "Get a list of all organisations of a certain type (buyer/supplier).";
                operation.Summary = "Get a list of all organisations.";
                operation.Responses["200"].Description = "A list of organisations.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            });

        app.MapPost("/organisations",
            [OrganisationAuthorize([AuthenticationChannel.OneLogin])]
        async (RegisterOrganisation command, IUseCase<RegisterOrganisation, Model.Organisation> useCase) =>
              await useCase.Execute(command)
              .AndThen(organisation =>
                  organisation != null
                      ? Results.Created(new Uri($"/organisations/{organisation.Id}", UriKind.Relative), organisation)
                      : Results.Problem("Organisation could not be created due to an internal error")))
            .Produces<Model.Organisation>(StatusCodes.Status201Created, "application/json")
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithOpenApi(operation =>
        {
            operation.OperationId = "CreateOrganisation";
            operation.Description = "Create a new organisation.";
            operation.Summary = "Create a new organisation.";
            operation.Responses["201"].Description = "Organisation created successfully.";
            operation.Responses["400"].Description = "Bad request.";
            operation.Responses["422"].Description = "Unprocessable entity.";
            operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
            operation.Responses["500"].Description = "Internal server error.";
            return operation;
        });

        app.MapGet("/organisations/{organisationId}",
            [OrganisationAuthorize(
                [AuthenticationChannel.OneLogin, AuthenticationChannel.ServiceKey],
                [Constants.OrganisationPersonScope.Admin, Constants.OrganisationPersonScope.Editor, Constants.OrganisationPersonScope.Viewer],
                OrganisationIdLocation.Path,
                [Constants.PersonScope.SupportAdmin],
                apiKeyScopes: [Constants.ApiKeyScopes.ReadOrganisationData])]
        async (Guid organisationId, IUseCase<Guid, Model.Organisation?> useCase) =>
            await useCase.Execute(organisationId)
                   .AndThen(organisation => organisation != null ? Results.Ok(organisation) : Results.NotFound()))
            .Produces<Model.Organisation>(StatusCodes.Status200OK, "application/json")
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
           {
               operation.OperationId = "GetOrganisation";
               operation.Description = "Get an organisation by ID.";
               operation.Summary = "Get an organisation by ID.";
               operation.Responses["200"].Description = "Organisation details.";
               operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
               operation.Responses["404"].Description = "Organisation not found.";
               operation.Responses["500"].Description = "Internal server error.";
               return operation;
           });

        app.MapGet("/organisations/{organisationId}/reviews",
            [OrganisationAuthorize(
                [AuthenticationChannel.OneLogin, AuthenticationChannel.ServiceKey],
                [Constants.OrganisationPersonScope.Admin, Constants.OrganisationPersonScope.Editor, Constants.OrganisationPersonScope.Viewer],
                OrganisationIdLocation.Path,
                [Constants.PersonScope.SupportAdmin])]
        async (Guid organisationId, IUseCase<Guid, IEnumerable<Review>> useCase) =>
        await useCase.Execute(organisationId)
                   .AndThen(reviews => reviews.Count() != 0 ? Results.Ok(reviews) : Results.NotFound()))
            .Produces<IEnumerable<Review>>(StatusCodes.Status200OK, "application/json")
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "GetOrganisationReviews";
                operation.Description = "Get organisation reviews by organisation ID.";
                operation.Summary = "Get a organisation reviews by ID.";
                operation.Responses["200"].Description = "Organisation reviews.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["404"].Description = "Organisation reviews not found.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            });

        app.MapPatch("/organisations/{organisationId}",
            [OrganisationAuthorize(
                [AuthenticationChannel.OneLogin],
                [Constants.OrganisationPersonScope.Admin, Constants.OrganisationPersonScope.Editor],
                OrganisationIdLocation.Path)]
        async (Guid organisationId, UpdateOrganisation updateOrganisation,
                IUseCase<(Guid, UpdateOrganisation), bool> useCase) =>
                    await useCase.Execute((organisationId, updateOrganisation))
                        .AndThen(_ => Results.NoContent()))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "UpdateOrganisation";
                operation.Description = "Update Organisation.";
                operation.Summary = "Update Organisation.";
                operation.Responses["204"].Description = "Organisation updated successfully.";
                operation.Responses["400"].Description = "Bad request.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["404"].Description = "Organisation not found.";
                operation.Responses["422"].Description = "Unprocessable entity.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            });

        app.MapPost("/organisations/{organisationId}/join-requests",
                [OrganisationAuthorize([AuthenticationChannel.OneLogin])]
        async (Guid organisationId, CreateOrganisationJoinRequest command, IUseCase<(Guid, CreateOrganisationJoinRequest), OrganisationJoinRequest> useCase) =>
                    await useCase.Execute((organisationId, command))
                        .AndThen(Results.Ok))
            .Produces<OrganisationJoinRequest>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "CreateJoinRequest";
                operation.Description = "Create a new organisation join request.";
                operation.Summary = "Create a new organisation join request.";
                operation.Responses["200"].Description = "Organisation join request created successfully.";
                operation.Responses["400"].Description = "Bad request.";
                operation.Responses["422"].Description = "Unprocessable entity.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            });

        app.MapGet("/organisations/{organisationId}/join-requests",
            [OrganisationAuthorize(
                [AuthenticationChannel.OneLogin],
                [Constants.OrganisationPersonScope.Admin],
                OrganisationIdLocation.Path)]
        async (Guid organisationId, [FromQuery] OrganisationJoinRequestStatus? status, IUseCase<(Guid, OrganisationJoinRequestStatus?), IEnumerable<JoinRequestLookUp>> useCase) =>
                await useCase.Execute((organisationId, status))
                    .AndThen(organisations => organisations != null ? Results.Ok(organisations) : Results.NotFound()))
            .Produces<List<JoinRequestLookUp>>(StatusCodes.Status200OK, "application/json")
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "GetOrganisationJoinRequests";
                operation.Description = "Get organisations join request by Organisation ID and status.";
                operation.Summary = "Get organisations join request by Organisation ID and status.";
                operation.Responses["200"].Description = "Organisations join requestdetails.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["404"].Description = "Organisations join request information not found.";
                operation.Responses["422"].Description = "Unprocessable entity.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            });

        app.MapPatch("/organisations/{organisationId}/join-requests/{joinRequestId}",
            [OrganisationAuthorize(
                [AuthenticationChannel.OneLogin],
                [Constants.OrganisationPersonScope.Admin],
                OrganisationIdLocation.Path)]
        async (Guid organisationId, Guid joinRequestId, UpdateJoinRequest updateJoinRequest, IUseCase<(Guid, Guid, UpdateJoinRequest), bool> useCase) =>

                 await useCase.Execute((organisationId, joinRequestId, updateJoinRequest))
                     .AndThen(_ => Results.NoContent())
         )
         .Produces(StatusCodes.Status200OK)
         .Produces(StatusCodes.Status204NoContent)
         .ProducesProblem(StatusCodes.Status400BadRequest)
         .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
         .ProducesProblem(StatusCodes.Status404NotFound)
         .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
         .ProducesProblem(StatusCodes.Status500InternalServerError)
         .WithOpenApi(operation =>
         {
             operation.OperationId = "UpdateOrganisationJoinRequest";
             operation.Description = "Update an organisation join request.";
             operation.Summary = "Update an organisation join request.";
             operation.Responses["200"].Description = "Organisation join request updated successfully.";
             operation.Responses["204"].Description = "Organisation join request updated successfully.";
             operation.Responses["400"].Description = "Bad request.";
             operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
             operation.Responses["404"].Description = "Organisation or join request not found.";
             operation.Responses["422"].Description = "Unprocessable entity.";
             operation.Responses["500"].Description = "Internal server error.";
             return operation;
         });
    }

    public static RouteGroupBuilder UseOrganisationLookupEndpoints(this RouteGroupBuilder app)
    {
        app.MapGet("/me",
            [OrganisationAuthorize([AuthenticationChannel.OrganisationKey])]
        async (IUseCase<Model.Organisation?> useCase) =>
                await useCase.Execute()
                    .AndThen(organisation => organisation != null ? Results.Ok(organisation) : Results.NotFound()))
            .Produces<List<Model.Organisation>>(StatusCodes.Status200OK, "application/json")
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "MyOrganisation";
                operation.Description = "The organisation details of the organisation the API key was issued for.";
                operation.Summary = "The organisation details of the organisation the API key was issued for.";
                operation.Tags = new List<OpenApiTag> { new() { Name = "Organisation - Lookup" } };
                operation.Responses["200"].Description = "Organisation details.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["404"].Description = "Organisation matching the API key was not found.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            });

        app.MapGet("/lookup",
            [OrganisationAuthorize([AuthenticationChannel.OneLogin, AuthenticationChannel.ServiceKey])]
        async ([FromQuery] string? name, [FromQuery] string? identifier, IUseCase<OrganisationQuery, Model.Organisation?> useCase) =>
                 await useCase.Execute(new OrganisationQuery(name, identifier))
                    .AndThen(organisation => organisation != null ? Results.Ok(organisation) : Results.NotFound()))
         .Produces<Model.Organisation>(StatusCodes.Status200OK, "application/json")
         .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
         .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
         .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
         .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
         .WithOpenApi(operation =>
         {
             operation.OperationId = "LookupOrganisation";
             operation.Description = "Find an organisation by name or identifier.";
             operation.Summary = "Find an organisation by name or identifier.";
             operation.Tags = new List<OpenApiTag> { new() { Name = "Organisation - Lookup" } };
             operation.Responses["200"].Description = "Organisation details.";
             operation.Responses["400"].Description = "Bad request.";
             operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
             operation.Responses["404"].Description = "Organisation not found.";
             operation.Responses["500"].Description = "Internal server error.";
             return operation;
         });

        app.MapGet("/search",
            [OrganisationAuthorize([AuthenticationChannel.OneLogin, AuthenticationChannel.ServiceKey])]
            async ([FromQuery] string name, [FromQuery] string? role, [FromQuery] int limit, [FromServices] IUseCase<OrganisationSearchQuery, IEnumerable<Model.OrganisationSearchResult>> useCase, [FromQuery] double? threshold = 0.3, [FromQuery] bool includePendingRoles = false) =>
            {
                if (threshold is < 0 or > 1)
                {
                    return Results.BadRequest(new ProblemDetails
                    {
                        Title = "Invalid threshold value",
                        Detail = "Threshold must be between 0 and 1.",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                return await useCase.Execute(new OrganisationSearchQuery(name, limit, threshold, role, includePendingRoles))
                    .AndThen(results => results.Count() != 0 ? Results.Ok(results) : Results.NotFound());
            })
            .Produces<IEnumerable<Model.OrganisationSearchResult>>(StatusCodes.Status200OK, "application/json")
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "SearchOrganisation";
                operation.Description = "Find organisations by partial matches on name.";
                operation.Summary = "Find organisations by partial matches on name.";
                operation.Tags = new List<OpenApiTag> { new() { Name = "Organisation - Lookup" } };
                operation.Responses["200"].Description = "Matching organisations.";
                operation.Responses["400"].Description = "Bad request.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["404"].Description = "No organisations found.";
                operation.Responses["500"].Description = "Internal server error.";

                foreach (var parameter in operation.Parameters)
                {
                    if (parameter.Name == "threshold")
                    {
                        parameter.Description = "The word similarity threshold value for fuzzy searching - Value can be from 0 to 1";
                    }

                    if (parameter.Name == "role")
                    {
                        parameter.Description = "Restrict results to role - tenderer or buyer";
                    }

                    if (parameter.Name == "limit")
                    {
                        parameter.Description = "Number of results to return";
                    }

                    if (parameter.Name == "threshold")
                    {
                        parameter.Description = "The word similarity threshold value for fuzzy searching - Value can be from 0 to 1";
                    }

                    if (parameter.Name == "includePendingRoles")
                    {
                        parameter.Description = "Include organisations with pending roles in the results";
                    }
                }

                return operation;
            });




        app.MapGet("/find/by-organisation-email",
                [OrganisationAuthorize([AuthenticationChannel.OneLogin, AuthenticationChannel.ServiceKey])]
                async ([FromQuery] string email, [FromQuery] string? role, [FromQuery] int limit, [FromServices] IUseCase<OrganisationsByOrganisationEmailQuery, IEnumerable<Model.OrganisationSearchResult>> useCase) =>
                await useCase.Execute(new OrganisationsByOrganisationEmailQuery(email, limit, role))
                    .AndThen(results => results.Count() != 0 ? Results.Ok(results) : Results.NotFound()))
            .Produces<IEnumerable<Model.OrganisationSearchResult>>(StatusCodes.Status200OK, "application/json")
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "FindOrganisationsByOrganisationEmail";
                operation.Description = "Find organisations by matching organisation email";
                operation.Summary = "Find organisations by matching organisation email";
                operation.Tags = new List<OpenApiTag> { new() { Name = "Organisation - Lookup" } };
                operation.Responses["200"].Description = "Matching organisations.";
                operation.Responses["400"].Description = "Bad request.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["404"].Description = "No organisations found.";
                operation.Responses["500"].Description = "Internal server error.";

                return operation;
            });

        app.MapGet("/find/by-admin-email",
                [OrganisationAuthorize([AuthenticationChannel.OneLogin, AuthenticationChannel.ServiceKey])]
                async ([FromQuery] string email, [FromQuery] string? role, [FromQuery] int limit, [FromServices] IUseCase<OrganisationsByAdminEmailQuery, IEnumerable<Model.OrganisationSearchResult>> useCase) =>
                await useCase.Execute(new OrganisationsByAdminEmailQuery(email, limit, role))
                    .AndThen(results => results.Count() != 0 ? Results.Ok(results) : Results.NotFound()))
            .Produces<IEnumerable<Model.OrganisationSearchResult>>(StatusCodes.Status200OK, "application/json")
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "FindOrganisationsByAdminEmail";
                operation.Description = "Find organisations by matching admin email";
                operation.Summary = "Find organisations by matching admin email";
                operation.Tags = new List<OpenApiTag> { new() { Name = "Organisation - Lookup" } };
                operation.Responses["200"].Description = "Matching organisations.";
                operation.Responses["400"].Description = "Bad request.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["404"].Description = "No organisations found.";
                operation.Responses["500"].Description = "Internal server error.";

                return operation;
            });

        return app;
    }

    public static RouteGroupBuilder UseSupportEndpoints(this RouteGroupBuilder app)
    {
        app.MapPatch("/organisation/{organisationId}",
            [OrganisationAuthorize([AuthenticationChannel.OneLogin], personScopes: [Constants.PersonScope.SupportAdmin])]
        async (Guid organisationId, SupportUpdateOrganisation supportUpdateOrganisation, IUseCase<(Guid, SupportUpdateOrganisation), bool> useCase) =>
                await useCase.Execute((organisationId, supportUpdateOrganisation))
                    .AndThen(Results.Ok))
            .Produces<Boolean>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "SupportUpdateOrganisation";
                operation.Description = "Update an organisation as a support admin. Perform support admin specific actions.";
                operation.Summary = "Updates an organisation.";
                operation.Responses["200"].Description = "Organisation updated successfully.";
                operation.Responses["400"].Description = "Bad request.";
                operation.Responses["422"].Description = "Unprocessable entity.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["404"].Description = "Organisation not found.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            });

        return app;
    }

    public static RouteGroupBuilder UseFeedbackEndpoints(this RouteGroupBuilder app)
    {
        app.MapPost("/feedback/",
            async (ProvideFeedbackAndContact feedback, IUseCase<ProvideFeedbackAndContact, bool> useCase) =>
                    await useCase.Execute(feedback)
                        .AndThen(Results.Ok))
                .Produces<Boolean>(StatusCodes.Status200OK, "application/json")
                .ProducesProblem(StatusCodes.Status500InternalServerError)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithOpenApi(operation =>
                {
                    operation.OperationId = "FeedbackAndContact";
                    operation.Description = "Contact the Find a Tender service team";
                    operation.Summary = "Ask a question, report a problem or suggest an improvement to the Find a Tender service team.";
                    operation.Responses["200"].Description = "Feedback sent successfully.";
                    operation.Responses["400"].Description = "Bad request.";
                    operation.Responses["500"].Description = "Internal server error.";
                    return operation;
                });

        app.MapPost("/contact-us/",
            async (ContactUs contactUs, IUseCase<ContactUs, bool> useCase) =>
                    await useCase.Execute(contactUs)
                        .AndThen(Results.Ok))
                .Produces<bool>(StatusCodes.Status200OK, "application/json")
                .ProducesProblem(StatusCodes.Status500InternalServerError)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithOpenApi(operation =>
                {
                    operation.OperationId = "ContactUs";
                    operation.Description = "Contact the Find a Tender service team";
                    operation.Summary = "Ask a question, report a problem or suggest an improvement to the Find a Tender service team.";
                    operation.Responses["200"].Description = "Feedback sent successfully.";
                    operation.Responses["400"].Description = "Bad request.";
                    operation.Responses["500"].Description = "Internal server error.";
                    return operation;
                });

        return app;
    }

    public static RouteGroupBuilder UseBuyerInformationEndpoints(this RouteGroupBuilder app)
    {
        app.MapPatch("/{organisationId}/buyer-information",
            [OrganisationAuthorize(
                [AuthenticationChannel.OneLogin],
                [Constants.OrganisationPersonScope.Admin, Constants.OrganisationPersonScope.Editor],
                OrganisationIdLocation.Path)]
        async (Guid organisationId, UpdateBuyerInformation buyerInformation,
                IUseCase<(Guid, UpdateBuyerInformation), bool> useCase) =>

                await useCase.Execute((organisationId, buyerInformation))
                   .AndThen(_ => Results.NoContent())
            )
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "UpdateBuyerInformation";
                operation.Description = "Update Buyer Information.";
                operation.Summary = "Update Buyer Information.";
                operation.Responses["204"].Description = "Buyer information updated successfully.";
                operation.Responses["400"].Description = "Bad request.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            });

        app.MapGet("/{organisationId}/buyer-information",
                [OrganisationAuthorize(
                    [AuthenticationChannel.OneLogin],
                    [Constants.OrganisationPersonScope.Admin, Constants.OrganisationPersonScope.Editor, Constants.OrganisationPersonScope.Viewer],
                    OrganisationIdLocation.Path,
                    [Constants.PersonScope.SupportAdmin])]
        async (Guid organisationId, IUseCase<Guid, BuyerInformation?> useCase) =>
                    await useCase.Execute(organisationId)
                        .AndThen(buyer => buyer != null ? Results.Ok(buyer) : Results.NotFound()))
            .Produces<BuyerInformation>(StatusCodes.Status200OK, "application/json")
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "GetOrganisationBuyerInformation";
                operation.Description = "Get organisation buyer information by ID.";
                operation.Summary = "Get organisation buyer information by ID.";
                operation.Responses["200"].Description = "Organisation buyer information details.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["404"].Description = "Organisation buyer information not found.";
                operation.Responses["422"].Description = "Unprocessable entity.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            });

        return app;
    }

    public static RouteGroupBuilder UseSupplierInformationEndpoints(this RouteGroupBuilder app)
    {
        app.MapGet("/{organisationId}/supplier-information",
            [OrganisationAuthorize(
                [AuthenticationChannel.OneLogin],
                [Constants.OrganisationPersonScope.Admin, Constants.OrganisationPersonScope.Editor, Constants.OrganisationPersonScope.Viewer],
                OrganisationIdLocation.Path,
                [Constants.PersonScope.SupportAdmin])]
        async (Guid organisationId, IUseCase<Guid, SupplierInformation?> useCase) =>
               await useCase.Execute(organisationId)
                   .AndThen(supplier => supplier != null ? Results.Ok(supplier) : Results.NotFound()))
           .Produces<SupplierInformation>(StatusCodes.Status200OK, "application/json")
           .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
           .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
           .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
           .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
           .WithOpenApi(operation =>
           {
               operation.OperationId = "GetOrganisationSupplierInformation";
               operation.Description = "Get organisation supplier information by ID.";
               operation.Summary = "Get organisation supplier information by ID.";
               operation.Responses["200"].Description = "Organisation supplier information details.";
               operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
               operation.Responses["404"].Description = "Organisation supplier information not found.";
               operation.Responses["422"].Description = "Unprocessable entity.";
               operation.Responses["500"].Description = "Internal server error.";
               return operation;
           });

        app.MapPatch("/{organisationId}/supplier-information",
            [OrganisationAuthorize(
                [AuthenticationChannel.OneLogin],
                [Constants.OrganisationPersonScope.Admin, Constants.OrganisationPersonScope.Editor],
                OrganisationIdLocation.Path)]
        async (Guid organisationId, UpdateSupplierInformation supplierInformation,
                IUseCase<(Guid, UpdateSupplierInformation), bool> useCase) =>
                    await useCase.Execute((organisationId, supplierInformation))
                        .AndThen(_ => Results.NoContent()))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "UpdateSupplierInformation";
                operation.Description = "Update Supplier Information.";
                operation.Summary = "Update Supplier Information.";
                operation.Responses["204"].Description = "Supplier information updated successfully.";
                operation.Responses["400"].Description = "Bad request.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["404"].Description = "Organisation supplier information not found.";
                operation.Responses["422"].Description = "Unprocessable entity.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            });

        return app;
    }

    public static RouteGroupBuilder UseConnectedEntityEndpoints(this RouteGroupBuilder app)
    {
        app.MapGet("/{organisationId}/connected-entities",
            [OrganisationAuthorize(
                [AuthenticationChannel.OneLogin],
                [Constants.OrganisationPersonScope.Admin, Constants.OrganisationPersonScope.Editor, Constants.OrganisationPersonScope.Viewer],
                OrganisationIdLocation.Path,
                [Constants.PersonScope.SupportAdmin])]
        async (Guid organisationId, IUseCase<Guid, IEnumerable<ConnectedEntityLookup>> useCase) =>
               await useCase.Execute(organisationId)
                   .AndThen(entities => entities != null ? Results.Ok(entities) : Results.NotFound()))
           .Produces<List<ConnectedEntityLookup>>(StatusCodes.Status200OK, "application/json")
           .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
           .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
           .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
           .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
           .WithOpenApi(operation =>
           {
               operation.OperationId = "GetConnectedEntities";
               operation.Description = "Get connected entities summary by Organisation ID.";
               operation.Summary = "Get connected entities information by Organisation ID.";
               operation.Responses["200"].Description = "Connected entities details.";
               operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
               operation.Responses["404"].Description = "Connected entities information not found.";
               operation.Responses["422"].Description = "Unprocessable entity.";
               operation.Responses["500"].Description = "Internal server error.";
               return operation;
           });

        app.MapGet("/{organisationId}/connected-entities/{connectedEntityId}",
            [OrganisationAuthorize(
                [AuthenticationChannel.OneLogin],
                [Constants.OrganisationPersonScope.Admin, Constants.OrganisationPersonScope.Editor, Constants.OrganisationPersonScope.Viewer],
                OrganisationIdLocation.Path,
                [Constants.PersonScope.SupportAdmin])]
        async (Guid organisationId, Guid connectedEntityId, IUseCase<(Guid, Guid), ConnectedEntity?> useCase) =>
               await useCase.Execute((organisationId, connectedEntityId))
                   .AndThen(entity => entity != null ? Results.Ok(entity) : Results.NotFound()))
            .Produces<ConnectedEntity>(StatusCodes.Status200OK, "application/json")
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "GetConnectedEntity";
                operation.Description = "Get connected entity by ID.";
                operation.Summary = "Get connected entity by ID.";
                operation.Responses["200"].Description = "Connected entity details.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["404"].Description = "Connected entity not found.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            });

        app.MapPost("/{organisationId}/connected-entities",
            [OrganisationAuthorize(
                [AuthenticationChannel.OneLogin],
                [Constants.OrganisationPersonScope.Admin, Constants.OrganisationPersonScope.Editor],
                OrganisationIdLocation.Path)]
        async (Guid organisationId, RegisterConnectedEntity updateConnectedEntity,
                IUseCase<(Guid, RegisterConnectedEntity), bool> useCase) =>

                await useCase.Execute((organisationId, updateConnectedEntity))
                    .AndThen(_ => Results.NoContent())
            )
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "CreateConnectedEntity";
                operation.Description = "Create a new connected entity.";
                operation.Summary = "Create a new connected entity.";
                operation.Responses["204"].Description = "Connected entity created successfully.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["404"].Description = "Connected entity not found.";
                operation.Responses["422"].Description = "Unprocessable entity.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            });

        app.MapPut("/{organisationId}/connected-entities/{connectedEntityId}",
            [OrganisationAuthorize(
                [AuthenticationChannel.OneLogin],
                [Constants.OrganisationPersonScope.Admin, Constants.OrganisationPersonScope.Editor],
                OrganisationIdLocation.Path)]
        async (Guid organisationId, Guid connectedEntityId, UpdateConnectedEntity updateConnectedEntity,
                        IUseCase<(Guid, Guid, UpdateConnectedEntity), bool> useCase) =>

                    await useCase.Execute((organisationId, connectedEntityId, updateConnectedEntity))
                        .AndThen(_ => Results.NoContent())
            )
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "UpdateConnectedEntity";
                operation.Description = "Updates a connected entity.";
                operation.Summary = "Update a connected entity.";
                operation.Responses["200"].Description = "Connected entity updated successfully.";
                operation.Responses["204"].Description = "Connected entity updated successfully.";
                operation.Responses["400"].Description = "Bad request.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["404"].Description = "Connected entity not found.";
                operation.Responses["422"].Description = "Unprocessable entity.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            });

        app.MapDelete("/{organisationId}/connected-entities/{connectedEntityId}",
            [OrganisationAuthorize(
                [AuthenticationChannel.OneLogin],
                [Constants.OrganisationPersonScope.Admin, Constants.OrganisationPersonScope.Editor],
                OrganisationIdLocation.Path)]
        async (Guid organisationId, Guid connectedEntityId,
                        IUseCase<(Guid, Guid), DeleteConnectedEntityResult> useCase) =>
                    await useCase.Execute((organisationId, connectedEntityId))
                        .AndThen(entity => entity != null ? Results.Ok(entity) : Results.NoContent())
            )
            .Produces<DeleteConnectedEntityResult>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "DeleteConnectedEntity";
                operation.Description = "Delete Connected Entity.";
                operation.Summary = "Delete Connected Entity.";
                operation.Responses["400"].Description = "Bad request.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["404"].Description = "Connected Entity not found.";
                operation.Responses["422"].Description = "Unprocessable entity.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            });

        return app;
    }

    public static RouteGroupBuilder UsePersonsEndpoints(this RouteGroupBuilder app)
    {
        app.MapGet("/{organisationId}/persons",
            [OrganisationAuthorize(
                [AuthenticationChannel.OneLogin],
                [Constants.OrganisationPersonScope.Admin],
                OrganisationIdLocation.Path,
                [Constants.PersonScope.SupportAdmin])]
        async (Guid organisationId, IUseCase<Guid, IEnumerable<Person>> useCase) =>
                    await useCase.Execute(organisationId)
                        .AndThen(persons => persons != null ? Results.Ok(persons) : Results.NotFound()))
            .Produces<List<Model.Person>>(StatusCodes.Status200OK, "application/json")
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "GetOrganisationPersons";
                operation.Description = "Get persons by Organisation ID.";
                operation.Summary = "Get persons by Organisation ID.";
                operation.Responses["200"].Description = "Persons details.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["404"].Description = "Persons information not found.";
                operation.Responses["422"].Description = "Unprocessable entity.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            });

        app.MapGet("/{organisationId}/persons-in-role",
            [OrganisationAuthorize(
                [AuthenticationChannel.OneLogin],
                [Constants.OrganisationPersonScope.Admin, Constants.OrganisationPersonScope.Editor, Constants.OrganisationPersonScope.Viewer],
                OrganisationIdLocation.Path,
                [Constants.PersonScope.SupportAdmin])]
        async (Guid organisationId, string role, IUseCase<(Guid, string), IEnumerable<Person>> useCase) =>
                    await useCase.Execute((organisationId, role))
                        .AndThen(persons => persons != null ? Results.Ok(persons) : Results.NotFound()))
            .Produces<List<Model.Person>>(StatusCodes.Status200OK, "application/json")
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "GetOrganisationPersonsInRole";
                operation.Description = "Get persons by Organisation ID in a role.";
                operation.Summary = "Get persons by Organisation ID in a role.";
                operation.Responses["200"].Description = "Persons details.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["404"].Description = "Persons information not found.";
                operation.Responses["422"].Description = "Unprocessable entity.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            });

        app.MapPatch("/{organisationId}/persons/{personId}",
            [OrganisationAuthorize(
                [AuthenticationChannel.OneLogin],
                [Constants.OrganisationPersonScope.Admin],
                OrganisationIdLocation.Path)]
        async (Guid organisationId, Guid personId, UpdatePersonToOrganisation updatePersonToOrganisation, IUseCase<(Guid, Guid, UpdatePersonToOrganisation), bool> useCase) =>

                 await useCase.Execute((organisationId, personId, updatePersonToOrganisation))
                     .AndThen(_ => Results.NoContent())
         )
         .Produces(StatusCodes.Status200OK)
         .Produces(StatusCodes.Status204NoContent)
         .ProducesProblem(StatusCodes.Status400BadRequest)
         .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
         .ProducesProblem(StatusCodes.Status404NotFound)
         .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
         .ProducesProblem(StatusCodes.Status500InternalServerError)
         .WithOpenApi(operation =>
         {
             operation.OperationId = "UpdateOrganisationPerson";
             operation.Description = "Update a organisation person.";
             operation.Summary = "Update organisation person.";
             operation.Responses["200"].Description = "Organisation Person updated successfully.";
             operation.Responses["204"].Description = "Organisation Person updated successfully.";
             operation.Responses["400"].Description = "Bad request.";
             operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
             operation.Responses["404"].Description = "Organisation or Person not found.";
             operation.Responses["422"].Description = "Unprocessable entity.";
             operation.Responses["500"].Description = "Internal server error.";
             return operation;
         });

        app.MapDelete("/{organisationId}/persons",
            [OrganisationAuthorize(
                [AuthenticationChannel.OneLogin],
                [Constants.OrganisationPersonScope.Admin],
                OrganisationIdLocation.Path)]
        async (Guid organisationId, [FromBody] RemovePersonFromOrganisation removePersonFromOrganisation, IUseCase<(Guid, RemovePersonFromOrganisation), bool> useCase) =>
                    await useCase.Execute((organisationId, removePersonFromOrganisation))
            .AndThen(_ => Results.NoContent()))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "RemovePersonFromOrganisation";
                operation.Description = "Remove person from organisation.";
                operation.Summary = "Remove person from organisation.";
                operation.Responses["204"].Description = "Person removed from organisation successfully.";
                operation.Responses["400"].Description = "Bad request.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["404"].Description = "Person not found.";
                operation.Responses["422"].Description = "Unprocessable entity.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            });

        app.MapGet("/{organisationId}/invites",
            [OrganisationAuthorize(
                [AuthenticationChannel.OneLogin],
                [Constants.OrganisationPersonScope.Admin],
                OrganisationIdLocation.Path)]
        async (Guid organisationId, IUseCase<Guid, IEnumerable<PersonInviteModel>> useCase) =>
                    await useCase.Execute(organisationId)
                        .AndThen(personInvites => personInvites != null ? Results.Ok(personInvites) : Results.NotFound()))
            .Produces<List<PersonInviteModel>>(StatusCodes.Status200OK, "application/json")
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "GetOrganisationPersonInvites";
                operation.Description = "Get unclaimed person invites by Organisation ID.";
                operation.Summary = "Get unclaimed person invites by Organisation ID.";
                operation.Responses["200"].Description = "Person invite details.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["404"].Description = "Person invite information not found.";
                operation.Responses["422"].Description = "Unprocessable entity.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            });

        app.MapPost("/{organisationId}/invites",
            [OrganisationAuthorize(
                [AuthenticationChannel.OneLogin],
                [Constants.OrganisationPersonScope.Admin],
                OrganisationIdLocation.Path)]
        async (Guid organisationId, InvitePersonToOrganisation invitePersonToOrganisation, IUseCase<(Guid, InvitePersonToOrganisation), bool> useCase) =>

                    await useCase.Execute((organisationId, invitePersonToOrganisation))
                        .AndThen(Results.Ok)
            )
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "CreatePersonInvite";
                operation.Description = "Create a new person invite.";
                operation.Summary = "Create a new person invite.";
                operation.Responses["200"].Description = "Person invite created successfully.";
                operation.Responses["400"].Description = "Bad request.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["404"].Description = "Organisation not found.";
                operation.Responses["422"].Description = "Unprocessable entity.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            });

        app.MapPatch("/{organisationId}/invites/{personInviteId}",
            [OrganisationAuthorize(
                [AuthenticationChannel.OneLogin],
                [Constants.OrganisationPersonScope.Admin],
                OrganisationIdLocation.Path)]
        async (Guid organisationId, Guid personInviteId, UpdateInvitedPersonToOrganisation updateInvitedPersonToOrganisation, IUseCase<(Guid, Guid, UpdateInvitedPersonToOrganisation), bool> useCase) =>

                 await useCase.Execute((organisationId, personInviteId, updateInvitedPersonToOrganisation))
                     .AndThen(_ => Results.NoContent())
         )
         .Produces(StatusCodes.Status200OK)
         .Produces(StatusCodes.Status204NoContent)
         .ProducesProblem(StatusCodes.Status400BadRequest)
         .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
         .ProducesProblem(StatusCodes.Status404NotFound)
         .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
         .ProducesProblem(StatusCodes.Status500InternalServerError)
         .WithOpenApi(operation =>
         {
             operation.OperationId = "UpdatePersonInvite";
             operation.Description = "Update a person invite.";
             operation.Summary = "Update a new person invite.";
             operation.Responses["200"].Description = "Person invite updated successfully.";
             operation.Responses["204"].Description = "Person invite updated successfully.";
             operation.Responses["400"].Description = "Bad request.";
             operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
             operation.Responses["404"].Description = "Organisation or Person Invite not found.";
             operation.Responses["422"].Description = "Unprocessable entity.";
             operation.Responses["500"].Description = "Internal server error.";
             return operation;
         });

        app.MapDelete("/{organisationId}/invites/{personInviteId}",
            [OrganisationAuthorize(
                [AuthenticationChannel.OneLogin],
                [Constants.OrganisationPersonScope.Admin],
                OrganisationIdLocation.Path)]
        async (Guid organisationId, Guid personInviteId, IUseCase<(Guid, Guid), bool> useCase) =>
                    await useCase.Execute((organisationId, personInviteId))
                        .AndThen(_ => Results.NoContent()))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "RemovePersonInviteFromOrganisation";
                operation.Description = "Remove person invite from organisation.";
                operation.Summary = "Remove person invite from organisation.";
                operation.Responses["204"].Description = "Person invite removed from organisation successfully.";
                operation.Responses["400"].Description = "Bad request.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["404"].Description = "Person invite not found.";
                operation.Responses["422"].Description = "Unprocessable entity.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            });

        return app;
    }

    public static RouteGroupBuilder UseManageApiKeyEndpoints(this RouteGroupBuilder app)
    {
        app.MapGet("/{organisationId}/api-keys",
            [OrganisationAuthorize(
                [AuthenticationChannel.OneLogin],
                [Constants.OrganisationPersonScope.Admin, Constants.OrganisationPersonScope.Editor, Constants.OrganisationPersonScope.Viewer],
                OrganisationIdLocation.Path,
                [Constants.PersonScope.SupportAdmin])]
        async (Guid organisationId, IUseCase<Guid, IEnumerable<Model.AuthenticationKey>> useCase) =>
               await useCase.Execute(organisationId)
                   .AndThen(entities => Results.Ok(entities)))
           .Produces<List<AuthenticationKey>>(StatusCodes.Status200OK, "application/json")
           .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
           .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
           .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
           .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
           .WithOpenApi(operation =>
           {
               operation.OperationId = "GetAuthenticationKeys";
               operation.Description = "Get authentication keys by Organisation ID.";
               operation.Summary = "Get authentication keys information by Organisation ID.";
               operation.Responses["200"].Description = "Authentication keys details.";
               operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
               operation.Responses["404"].Description = "authentication keys information not found.";
               operation.Responses["422"].Description = "Unprocessable entity.";
               operation.Responses["500"].Description = "Internal server error.";
               return operation;
           });

        app.MapPost("/{organisationId}/api-keys",
            [OrganisationAuthorize(
                [AuthenticationChannel.OneLogin],
                [Constants.OrganisationPersonScope.Admin, Constants.OrganisationPersonScope.Editor],
                OrganisationIdLocation.Path)]
        async (Guid organisationId, RegisterAuthenticationKey registerAuthenticationKey,
                IUseCase<(Guid, RegisterAuthenticationKey), bool> useCase) =>

                await useCase.Execute((organisationId, registerAuthenticationKey))
                    .AndThen(_ => Results.Created())
            )
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "CreateAuthenticationKey";
                operation.Description = "Create a new authentication key.";
                operation.Summary = "Create a new authentication key.";
                operation.Responses["201"].Description = "Authentication key created successfully.";
                operation.Responses["400"].Description = "Bad request.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["404"].Description = "Authentication failed.";
                operation.Responses["422"].Description = "Unprocessable entity.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            });

        app.MapPatch("/{organisationId}/api-keys/{keyName}/revoke",
            [OrganisationAuthorize(
                [AuthenticationChannel.OneLogin],
                [Constants.OrganisationPersonScope.Admin, Constants.OrganisationPersonScope.Editor],
                OrganisationIdLocation.Path)]
        async (Guid organisationId, string keyName,
                IUseCase<(Guid, string), bool> useCase) =>
                    await useCase.Execute((organisationId, keyName))
                        .AndThen(_ => Results.NoContent()))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "RevokeAuthenticationKey";
                operation.Description = "Revoke Authentication key.";
                operation.Summary = "Revoke Authentication key.";
                operation.Responses["204"].Description = "Authentication key revoked successfully.";
                operation.Responses["400"].Description = "Bad request.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["404"].Description = "Authentication failed.";
                operation.Responses["422"].Description = "Unprocessable entity.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            });

        return app;
    }

    public static RouteGroupBuilder UseOrganisationMouEndpoints(this RouteGroupBuilder app)
    {
        app.MapGet("/{organisationId}/mou",
        [OrganisationAuthorize(
            [AuthenticationChannel.OneLogin],
              [Constants.OrganisationPersonScope.Admin],
              OrganisationIdLocation.Path)]
        async (Guid organisationId, IUseCase<Guid, IEnumerable<MouSignature>> useCase) =>
                  await useCase.Execute(organisationId)
                      .AndThen(mouSignatures => mouSignatures != null ? Results.Ok(mouSignatures) : Results.NotFound()))
          .Produces<List<Model.MouSignature>>(StatusCodes.Status200OK, "application/json")
          .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
          .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
          .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
          .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
          .WithOpenApi(operation =>
          {
              operation.OperationId = "GetOrganisationMouSignatures";
              operation.Description = "Get MOU Signatures by Organisation ID.";
              operation.Summary = "Get MOU Signatures by Organisation ID.";
              operation.Responses["200"].Description = "MOU Signatures.";
              operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
              operation.Responses["404"].Description = "Mou Signatures not found.";
              operation.Responses["422"].Description = "Unprocessable entity.";
              operation.Responses["500"].Description = "Internal server error.";
              return operation;
          });

        app.MapGet("/{organisationId}/mou/{mouSignatureId}",
        [OrganisationAuthorize(
            [AuthenticationChannel.OneLogin])]
        async (Guid organisationId, Guid mouSignatureId, IUseCase<(Guid, Guid), MouSignature> useCase) =>
                  await useCase.Execute((organisationId, mouSignatureId))
                      .AndThen(mouSignature => mouSignature != null ? Results.Ok(mouSignature) : Results.NotFound()))
          .Produces<Model.MouSignature>(StatusCodes.Status200OK, "application/json")
          .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
          .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
          .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
          .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
          .WithOpenApi(operation =>
          {
              operation.OperationId = "GetOrganisationMouSignature";
              operation.Description = "Get MOU Signature by Organisation ID and Mou ID.";
              operation.Summary = "Get MOU Signature by Organisation ID and Mou ID.";
              operation.Responses["200"].Description = "MOU Signature.";
              operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
              operation.Responses["404"].Description = "Mou Signature not found.";
              operation.Responses["422"].Description = "Unprocessable entity.";
              operation.Responses["500"].Description = "Internal server error.";
              return operation;
          });

        app.MapGet("/{organisationId}/mou/latest",
      [OrganisationAuthorize(
            [AuthenticationChannel.OneLogin],
            [Constants.OrganisationPersonScope.Admin, Constants.OrganisationPersonScope.Responder, Constants.OrganisationPersonScope.Editor, Constants.OrganisationPersonScope.Viewer],
            OrganisationIdLocation.Path,
            [Constants.PersonScope.SupportAdmin])]
        async (Guid organisationId, IUseCase<Guid, MouSignatureLatest> useCase) =>
                await useCase.Execute(organisationId)
                    .AndThen(mouSignatureLatest => mouSignatureLatest != null ? Results.Ok(mouSignatureLatest) : Results.NotFound()))
        .Produces<Model.MouSignatureLatest>(StatusCodes.Status200OK, "application/json")
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
        .WithOpenApi(operation =>
        {
            operation.OperationId = "GetOrganisationLatestMouSignature";
            operation.Description = "Get Latest MOU Signature by Organisation ID and Mou ID.";
            operation.Summary = "Get Latest MOU Signature by Organisation ID and Mou ID.";
            operation.Responses["200"].Description = "Latest MOU Signature.";
            operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
            operation.Responses["404"].Description = "Latest Mou Signature not found.";
            operation.Responses["422"].Description = "Unprocessable entity.";
            operation.Responses["500"].Description = "Internal server error.";
            return operation;
        });

        app.MapPost("/{organisationId}/mou",
           [OrganisationAuthorize(
            [AuthenticationChannel.OneLogin],
            [Constants.OrganisationPersonScope.Admin],
              OrganisationIdLocation.Path)]
        async (Guid organisationId, SignMouRequest signMou, IUseCase<(Guid, SignMouRequest), bool> useCase) =>
                   await useCase.Execute((organisationId, signMou))
                       .AndThen(Results.Ok)
           )
           .Produces(StatusCodes.Status200OK)
           .ProducesProblem(StatusCodes.Status400BadRequest)
           .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
           .ProducesProblem(StatusCodes.Status404NotFound)
           .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
           .ProducesProblem(StatusCodes.Status500InternalServerError)
           .WithOpenApi(operation =>
           {
               operation.OperationId = "SignOrganisationMou";
               operation.Description = "Sign a mou for the organisation.";
               operation.Summary = "Sign a mou for the organisation.";
               operation.Responses["200"].Description = "Sign Mou created successfully.";
               operation.Responses["400"].Description = "Bad request.";
               operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
               operation.Responses["404"].Description = "Organisation not found.";
               operation.Responses["422"].Description = "Unprocessable entity.";
               operation.Responses["500"].Description = "Internal server error.";
               return operation;
           });
        return app;
    }

    public static RouteGroupBuilder UseMouEndpoints(this RouteGroupBuilder app)
    {
        app.MapGet("/latest",
       [OrganisationAuthorize(
         [AuthenticationChannel.OneLogin])]
        async (IUseCase<Mou> useCase) =>
             await useCase.Execute()
                 .AndThen(mouLatest => mouLatest != null ? Results.Ok(mouLatest) : Results.NotFound()))
     .Produces<Mou>(StatusCodes.Status200OK, "application/json")
     .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
     .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
     .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
     .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
     .WithOpenApi(operation =>
     {
         operation.OperationId = "GetLatestMou";
         operation.Description = "Get Latest MOU.";
         operation.Summary = "Get Latest MOU to sign.";
         operation.Responses["200"].Description = "Latest MOU.";
         operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
         operation.Responses["404"].Description = "Latest Mou information not found.";
         operation.Responses["422"].Description = "Unprocessable entity.";
         operation.Responses["500"].Description = "Internal server error.";
         return operation;
     });

        app.MapGet("/{mouId}",
       [OrganisationAuthorize(
          [AuthenticationChannel.OneLogin]
         , [Constants.OrganisationPersonScope.Admin],
         OrganisationIdLocation.Path
       )]
        async (Guid mouId, IUseCase<Guid, Mou> useCase) =>
          await useCase.Execute(mouId)
              .AndThen(mou => mou != null ? Results.Ok(mou) : Results.NotFound()))
      .Produces<Mou>(StatusCodes.Status200OK, "application/json")
      .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
      .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
      .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
      .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
      .WithOpenApi(operation =>
      {
          operation.OperationId = "GetMou";
          operation.Description = "Get MOU byId.";
          operation.Summary = "Get MOU by ID.";
          operation.Responses["200"].Description = "MOU by Id.";
          operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
          operation.Responses["404"].Description = "Mou information not found.";
          operation.Responses["422"].Description = "Unprocessable entity.";
          operation.Responses["500"].Description = "Internal server error.";
          return operation;
      });

        return app;
    }

    public static RouteGroupBuilder UseOrganisationPartiesEndpoints(this RouteGroupBuilder app)
    {
        app.MapGet("/{organisationId}/parties",
            [OrganisationAuthorize(
                [AuthenticationChannel.OneLogin],
                [Constants.OrganisationPersonScope.Admin, Constants.OrganisationPersonScope.Editor, Constants.OrganisationPersonScope.Responder, Constants.OrganisationPersonScope.Viewer],
                OrganisationIdLocation.Path,
                [Constants.PersonScope.SupportAdmin])]
        async (Guid organisationId, IUseCase<Guid, OrganisationParties?> useCase) =>
                await useCase.Execute(organisationId)
                   .AndThen(parties => parties == null ? Results.NotFound() : Results.Ok(parties))
            )
            .Produces<OrganisationParties>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "GetOrganisationParties";
                operation.Description = "Get organisation parties";
                operation.Summary = "Get organisation parties";
                operation.Responses["200"].Description = "Organisation parties.";
                operation.Responses["400"].Description = "Bad request.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["404"].Description = "Organisation parties not found.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            });

        app.MapPost("/{organisationId}/add-party",
            [OrganisationAuthorize(
                [AuthenticationChannel.OneLogin],
                [Constants.OrganisationPersonScope.Admin, Constants.OrganisationPersonScope.Editor],
                OrganisationIdLocation.Path)]
        async (Guid organisationId, AddOrganisationParty organisationParty, IUseCase<(Guid, AddOrganisationParty), bool> useCase) =>
                await useCase.Execute((organisationId, organisationParty))
                   .AndThen(_ => Results.NoContent())
            )
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "AddOrganisationParty";
                operation.Description = "Add organisation party";
                operation.Summary = "Add organisation party";
                operation.Responses["204"].Description = "Organisation party added successfully.";
                operation.Responses["400"].Description = "Bad request.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["404"].Description = "Organisation parties not found.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            });

        app.MapPost("/{organisationId}/update-party",
            [OrganisationAuthorize(
                [AuthenticationChannel.OneLogin],
                [Constants.OrganisationPersonScope.Admin, Constants.OrganisationPersonScope.Editor],
                OrganisationIdLocation.Path)]
        async (Guid organisationId, UpdateOrganisationParty organisationParty, IUseCase<(Guid, UpdateOrganisationParty), bool> useCase) =>
                await useCase.Execute((organisationId, organisationParty))
                   .AndThen(_ => Results.NoContent())
            )
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "UpdateOrganisationParty";
                operation.Description = "Update organisation party";
                operation.Summary = "Update organisation party";
                operation.Responses["204"].Description = "Organisation party updated successfully.";
                operation.Responses["400"].Description = "Bad request.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["404"].Description = "Organisation parties not found.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            });

        app.MapDelete("/{organisationId}/remove-party",
            [OrganisationAuthorize(
                [AuthenticationChannel.OneLogin],
                [Constants.OrganisationPersonScope.Admin, Constants.OrganisationPersonScope.Editor],
                OrganisationIdLocation.Path)]
        async (Guid organisationId, [FromBody]RemoveOrganisationParty organisationParty, IUseCase<(Guid, RemoveOrganisationParty), bool> useCase) =>
                await useCase.Execute((organisationId, organisationParty))
                   .AndThen(_ => Results.NoContent())
            )
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "RemoveOrganisationParty";
                operation.Description = "Remove organisation party";
                operation.Summary = "Remove organisation party";
                operation.Responses["204"].Description = "Organisation party removed successfully.";
                operation.Responses["400"].Description = "Bad request.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["404"].Description = "Organisation parties not found.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            });

        return app;
    }

    public static RouteGroupBuilder UseOrganisationHierarchyEndpoints(this RouteGroupBuilder app)
    {
        app.MapPost("/{organisationId}/hierarchy/child",
                [OrganisationAuthorize([AuthenticationChannel.OneLogin],
                    organisationPersonScopes: [Constants.OrganisationPersonScope.Admin, Constants.OrganisationPersonScope.Editor],
                    organisationIdLocation: OrganisationIdLocation.Path)]
                async (Guid organisationId, CreateParentChildRelationshipRequest request, IUseCase<CreateParentChildRelationshipRequest, CreateParentChildRelationshipResult> useCase) =>
                {
                    return await useCase.Execute(request)
                        .AndThen(result => result.Success
                            ? Results.Created($"/organisations/{organisationId}/hierarchy/child/{result.RelationshipId}", result)
                            : Results.Problem(statusCode: StatusCodes.Status400BadRequest));
                })
            .Produces<CreateParentChildRelationshipResult>(StatusCodes.Status201Created, "application/json")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "CreateParentChildRelationship";
                operation.Description = "Creates a parent-child relationship between two organisations.";
                operation.Summary = "Create a parent-child organisation relationship.";
                operation.Responses["201"].Description = "Parent-child relationship created successfully.";
                operation.Responses["400"].Description = "Bad request or failed to create relationship.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            });

        app.MapGet("/{organisationId}/hierarchy/children",
                [OrganisationAuthorize([AuthenticationChannel.OneLogin, AuthenticationChannel.ServiceKey],
                    organisationPersonScopes: [Constants.OrganisationPersonScope.Admin, Constants.OrganisationPersonScope.Editor, Constants.OrganisationPersonScope.Viewer],
                    organisationIdLocation: OrganisationIdLocation.Path)]
                async (Guid organisationId, IUseCase<Guid, GetChildOrganisationsResponse> useCase) =>
                    await useCase.Execute(organisationId)
                        .AndThen(response => response.Success
                            ? Results.Ok(response.ChildOrganisations)
                            : Results.Problem(statusCode: StatusCodes.Status500InternalServerError))
                        )
            .Produces<IEnumerable<OrganisationSummary>>(StatusCodes.Status200OK, "application/json")
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "GetChildOrganisations";
                operation.Description = "Retrieves all child organisations for a given parent organisation.";
                operation.Summary = "Get all child organisations of a parent organisation.";
                operation.Responses["200"].Description = "List of child organisations retrieved successfully.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            });

        app.MapDelete("/{organisationId}/hierarchy/child/{childOrganisationId}",
                [OrganisationAuthorize([AuthenticationChannel.OneLogin],
                    organisationPersonScopes: [Constants.OrganisationPersonScope.Admin, Constants.OrganisationPersonScope.Editor],
                    organisationIdLocation: OrganisationIdLocation.Path)]
                async (Guid organisationId, Guid childOrganisationId, [FromServices] ISupersedeChildOrganisationUseCase useCase) =>
                {
                    var request = new SupersedeChildOrganisationRequest
                    {
                        ParentOrganisationId = organisationId,
                        ChildOrganisationId = childOrganisationId
                    };

                    return await useCase.Execute(request)
                        .AndThen(result => result.Success
                            ? Results.NoContent()
                            : result.NotFound
                                ? Results.NotFound()
                                : Results.Problem(statusCode: StatusCodes.Status400BadRequest));
                })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "SupersedeChildOrganisation";
                operation.Description = "Supersedes a parent-child relationship between two organisations.";
                operation.Summary = "Supersede a parent-child organisation relationship.";
                operation.Responses["204"].Description = "Parent-child relationship superseded successfully.";
                operation.Responses["400"].Description = "Bad request or failed to supersede relationship.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["404"].Description = "Relationship not found.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            });

        app.MapGet("/{organisationId}/hierarchy/parent",
                [OrganisationAuthorize([AuthenticationChannel.OneLogin, AuthenticationChannel.ServiceKey],
                    organisationPersonScopes: [Constants.OrganisationPersonScope.Admin, Constants.OrganisationPersonScope.Editor, Constants.OrganisationPersonScope.Viewer],
                    organisationIdLocation: OrganisationIdLocation.Path)]
                async (Guid organisationId, IUseCase<Guid, GetParentOrganisationsResponse> useCase) =>
                    await useCase.Execute(organisationId)
                        .AndThen(response => response.Success
                            ? Results.Ok(response.ParentOrganisations)
                            : Results.Problem(statusCode: StatusCodes.Status500InternalServerError))
            )
            .Produces<IEnumerable<OrganisationSummary>>(StatusCodes.Status200OK, "application/json")
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "GetParentOrganisations";
                operation.Description = "Retrieves all parent organisations for a given child organisation.";
                operation.Summary = "Get all parent organisations of a child organisation.";
                operation.Responses["200"].Description = "List of parent organisations retrieved successfully.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            });

        return app;
    }

    public static RouteGroupBuilder useSearchRegistryOfPpon(this RouteGroupBuilder app)
    {
        app.MapGet("/search-by-name-or-ppon",
                [OrganisationAuthorize([AuthenticationChannel.OneLogin, AuthenticationChannel.ServiceKey])]
            async ([FromQuery] string searchText, [FromQuery] int limit, [FromQuery] int skip,[FromQuery] string sortOrder,
                [FromServices] IUseCase<OrganisationSearchByPponQuery, (IEnumerable<Model.OrganisationSearchByPponResult>, int)> useCase,
                [FromQuery] double? threshold = 0.3) =>
            {
                sortOrder = string.IsNullOrEmpty(sortOrder) ? "rel" : sortOrder;

                return await useCase.Execute(new OrganisationSearchByPponQuery(searchText, limit, skip, sortOrder, threshold))
                    .AndThen(result => {
                        var (results, totalCount) = result;
                        return results.Any()
                            ? Results.Ok(new { Results = results, TotalCount = totalCount })
                            : Results.NotFound();
                    });
            })
            .Produces<OrganisationSearchByPponResponse>(StatusCodes.Status200OK, "application/json")
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "SearchByNameOrPpon";
                operation.Description = "Find organisations by partial matches of name or ppon.";
                operation.Summary = "Find organisations by partial matches of name or ppon.";
                operation.Tags = new List<OpenApiTag> { new() { Name = "Organisation - Lookup" } };
                operation.Responses["200"].Description = "Matching organisations with total count.";
                operation.Responses["400"].Description = "Bad request.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["404"].Description = "No organisations found.";
                operation.Responses["500"].Description = "Internal server error.";

                foreach (var parameter in operation.Parameters)
                {
                    if (parameter.Name == "threshold")
                    {
                        parameter.Description = "The word similarity threshold value for fuzzy searching - Value can be from 0 to 1";
                    }
                    if (parameter.Name == "limit")
                    {
                        parameter.Description = "Number of results to return";
                    }
                    if (parameter.Name == "skip")
                    {
                        parameter.Description = "Number of results to skip for pagination";
                    }
                }
                return operation;
            });
        return app;
    }
}

public static class ApiExtensions
{
    public static void DocumentOrganisationApi(this SwaggerGenOptions options, IConfigurationManager configuration)
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = configuration.GetValue("Version", "dev"),
            Title = "Organisation Management API",
            Description = "API for creating, updating, deleting, and listing organisations, including a lookup feature against organisation identifiers."
        });
        options.CustomSchemaIds(type =>
        {
            var typeMap = new Dictionary<Type, string>
                {
                    { typeof(RegisterOrganisation), "NewOrganisation" },
                    { typeof(UpdateOrganisation), "UpdatedOrganisation" },
                };
            return typeMap.GetValueOrDefault(type, type.Name);
        }
       );
        options.IncludeXmlComments(Assembly.GetExecutingAssembly(), Assembly.GetAssembly(typeof(Address)));
        options.OperationFilter<ProblemDetailsOperationFilter>(ErrorCodes.Exception4xxMap.HttpStatusCodeErrorMap());
        options.ConfigureBearerSecurity();
        options.ConfigureApiKeySecurity();
        options.UseAllOfToExtendReferenceSchemas();
    }
}
