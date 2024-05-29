using System.Reflection;
using CO.CDP.Swashbuckle.SwaggerGen;
using DotSwashbuckle.AspNetCore.SwaggerGen;
using FluentAssertions;

namespace CO.CDP.Swashbuckle.Tests.SwaggerGen;

public class SwaggerGenOptionsExtensionsTest
{
    [Fact]
    public void ItConfiguresXmlCommentDescriptors()
    {
        var options = new SwaggerGenOptions();

        options.IncludeXmlComments(Assembly.GetExecutingAssembly());

        options.SchemaFilterDescriptors.Should().Contain(d => d.Type == typeof(XmlCommentsSchemaFilter));
    }
}