using CO.CDP.RegisterOfCommercialTools.WebApi.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.RegisterOfCommercialTools.WebApi.Api;

public static class CpvEndpoint
{
    public static IEndpointRouteBuilder MapCpvEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/filters/cpv/search", () => Results.Problem(statusCode: StatusCodes.Status501NotImplemented))
            .Produces(StatusCodes.Status501NotImplemented);

        app.MapGet("/filters/cpv/{code}/children", async (string code, [FromServices] GetCpvChildrenUseCase useCase) =>
            {
                var children = await useCase.ExecuteAsync(code);
                return Results.Ok(children);
            })
            .Produces<IEnumerable<CpvCode>>(StatusCodes.Status200OK);

        return app;
    }
}
