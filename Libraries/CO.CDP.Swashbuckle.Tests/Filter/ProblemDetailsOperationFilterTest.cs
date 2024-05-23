using CO.CDP.Swashbuckle.Filter;
using DotSwashbuckle.AspNetCore.SwaggerGen;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Numeric;
using FluentAssertions.Primitives;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace CO.CDP.Swashbuckle.Tests.Filter;

public class ProblemDetailsOperationFilterTest
{
    [Theory]
    [InlineData(400, "Bad Request", "https://tools.ietf.org/html/rfc7231#section-6.5.1")]
    [InlineData(401, "Unauthorized", "https://tools.ietf.org/html/rfc7235#section-3.1")]
    [InlineData(403, "Forbidden", "https://tools.ietf.org/html/rfc7231#section-6.5.3")]
    [InlineData(404, "Not Found", "https://tools.ietf.org/html/rfc7231#section-6.5.4")]
    [InlineData(406, "Not Acceptable", "https://tools.ietf.org/html/rfc7231#section-6.5.6")]
    [InlineData(409, "Conflict", "https://tools.ietf.org/html/rfc7231#section-6.5.8")]
    [InlineData(415, "Unsupported Media Type", "https://tools.ietf.org/html/rfc7231#section-6.5.13")]
    [InlineData(422, "Unprocessable Entity", "https://tools.ietf.org/html/rfc4918#section-11.2")]
    [InlineData(500, "Internal Server Error", "https://tools.ietf.org/html/rfc7231#section-6.6.1")]
    public void ItDocumentsErrorResponses(int status, string title, string type)
    {
        var operation = GivenOperationWithResponse($"{status}", "application/json");

        new ProblemDetailsOperationFilter().Apply(operation, OperationFilterContext());

        operation.Should().BeResponseOf($"{status}", "application/json")
            .And.Example.As<OpenApiObject>()
            .Should(o =>
            {
                using (new AssertionScope())
                {
                    o.Should().Contain("status", status);
                    o.Should().Contain("title", title);
                    o.Should().Contain("type", type);
                }
            });
    }

    [Fact]
    public void ItIgnoresUnusedStatusCodeResponses()
    {
        var operation = GivenOperationWithResponse("401", "application/json");

        new ProblemDetailsOperationFilter().Apply(operation, OperationFilterContext());

        operation.Responses.Should().ContainKey("401");
        operation.Responses.Should().NotContainKey("403");
    }

    [Fact]
    public void ItIgnoresUnusedMediaTypeResponses()
    {
        var operation = GivenOperationWithResponse("401", "application/xml");

        new ProblemDetailsOperationFilter().Apply(operation, OperationFilterContext());

        operation.Responses["401"].As<OpenApiResponse>().Content.Should().NotContainKey("application/json");
        operation.Responses["401"].As<OpenApiResponse>().Content.Should().ContainKey("application/xml");
        operation.Responses["401"].As<OpenApiResponse>().Content["application/xml"].Example.Should().NotBeNull();
    }

    private static OpenApiOperation GivenOperationWithResponse(string responseKey, string responseMediaType) =>
        new()
        {
            Responses =
            {
                {
                    responseKey, new OpenApiResponse
                    {
                        Content =
                        {
                            { responseMediaType, new OpenApiMediaType() }
                        }
                    }
                }
            }
        };

    private static OperationFilterContext OperationFilterContext()
    {
        return new OperationFilterContext(new ApiDescription(), null, null, null);
    }
}

public static class AssertionsExtensions
{
    public static void Should<T>(this T value, Action<T> assertions)
    {
        assertions(value);
    }
}

public static class OpenApiAssertionsExtensions
{
    public static OpenApiOperationAssertions Should(this OpenApiOperation operation)
    {
        return new OpenApiOperationAssertions(operation);
    }

    public static OpenApiObjectAssertions Should(this OpenApiObject value)
    {
        return new OpenApiObjectAssertions(value);
    }
}

public class OpenApiOperationAssertions(OpenApiOperation operation)
{
    public AndConstraint<OpenApiMediaType> BeResponseOf(string key, string mediaType)
    {
        return new AndConstraint<OpenApiMediaType>(
            operation.Responses[key].As<OpenApiResponse>()
                .Content[mediaType].As<OpenApiMediaType>()
        );
    }
}

public class OpenApiObjectAssertions(OpenApiObject value)
    : ReferenceTypeAssertions<OpenApiObject, OpenApiObjectAssertions>(value)
{
    private readonly OpenApiObject _value = value;

    public AndConstraint<StringAssertions> Contain(string name, string expectedValue)
    {
        return _value[name]
                .As<OpenApiString>()
                .Value
                .Should()
                .Be(expectedValue)
            ;
    }

    public AndConstraint<NumericAssertions<int>> Contain(string name, int expectedValue)
    {
        return _value[name]
                .As<OpenApiInteger>()
                .Value
                .Should()
                .Be(expectedValue)
            ;
    }

    protected override string Identifier => "OpenApiObject";
}