namespace CO.CDP.RegisterOfCommercialTools.WebApi.Api;

public static class CpvEndpoint
{
    public static IEndpointRouteBuilder MapCpvEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/filters/cpv/search", () => Results.Problem(statusCode: StatusCodes.Status501NotImplemented))
            .Produces(StatusCodes.Status501NotImplemented);

        app.MapGet("/filters/cpv/{code}/children", (string code) => Results.Problem(statusCode: StatusCodes.Status501NotImplemented))
            .Produces(StatusCodes.Status501NotImplemented);

        return app;
    }
}
