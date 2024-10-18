using CO.CDP.Authentication;
using CO.CDP.Authentication.Authorization;
using CO.CDP.Functional;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.Swashbuckle.Filter;
using CO.CDP.Swashbuckle.Security;
using CO.CDP.Swashbuckle.SwaggerGen;
using DotSwashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Address = CO.CDP.OrganisationInformation.Address;
using ConnectedEntity = CO.CDP.Organisation.WebApi.Model.ConnectedEntity;
using ConnectedEntityLookup = CO.CDP.Organisation.WebApi.Model.ConnectedEntityLookup;
using Person = CO.CDP.Organisation.WebApi.Model.Person;
using PersonInvite = CO.CDP.OrganisationInformation.Persistence.PersonInvite;

namespace CO.CDP.Organisation.WebApi.Api;

public static class EndpointExtensions
{
    public static void UseOrganisationEndpoints(this WebApplication app)
    {
        app.MapGet("/organisations",
            [OrganisationAuthorize([AuthenticationChannel.OneLogin], personScopes: [Constants.PersonScope.SupportAdmin])]
        async ([FromQuery] string type, [FromQuery] int limit, [FromQuery] int skip, IUseCase<PaginatedOrganisationQuery, IEnumerable<OrganisationExtended>> useCase) =>
                {
                    return await useCase.Execute(new PaginatedOrganisationQuery
                    {
                        Type = type,
                        Limit = limit,
                        Skip = skip
                    })
                        .AndThen(organisations => Results.Ok(organisations));
                })
            .Produces<List<OrganisationExtended>>(StatusCodes.Status200OK, "application/json")
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "GetAllOrganisations";
                operation.Description = "Get a list of all organisations.";
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
                [Constants.PersonScope.SupportAdmin])]
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
                        .AndThen(_ => Results.Created()))
            .Produces<OrganisationJoinRequest>(StatusCodes.Status201Created, "application/json")
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
                operation.Responses["201"].Description = "Organisation join request created successfully.";
                operation.Responses["400"].Description = "Bad request.";
                operation.Responses["422"].Description = "Unprocessable entity.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
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
            async (ProvideFeedback feedback, IUseCase<ProvideFeedback, bool> useCase) =>
                    await useCase.Execute(feedback)
                        .AndThen(Results.Ok))
                .Produces<Boolean>(StatusCodes.Status200OK, "application/json")
                .ProducesProblem(StatusCodes.Status500InternalServerError)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithOpenApi(operation =>
                {
                    operation.OperationId = "ProvideFeedback";
                    operation.Description = "Provide find a tender feedback";
                    operation.Summary = "Provide find a tender feedback.";
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
        async (Guid organisationId, Guid connectedEntityId, [FromBody] DeleteConnectedEntity deleteConnectedEntity,
                        IUseCase<(Guid, Guid, DeleteConnectedEntity), bool> useCase) =>
                    await useCase.Execute((organisationId, connectedEntityId, deleteConnectedEntity))
                        .AndThen(_ => Results.NoContent()))
            .Produces(StatusCodes.Status204NoContent)
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
                operation.Responses["204"].Description = "Connected Entity deleted successfully.";
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
                OrganisationIdLocation.Path)]
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
        async (Guid organisationId, InvitePersonToOrganisation invitePersonToOrganisation, IUseCase<(Guid, InvitePersonToOrganisation), PersonInvite> useCase) =>

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
        options.OperationFilter<ProblemDetailsOperationFilter>(Extensions.ServiceCollectionExtensions.ErrorCodes());
        options.ConfigureBearerSecurity();
        options.ConfigureApiKeySecurity();
        options.UseAllOfToExtendReferenceSchemas();
    }
}