using System.Collections.Generic;
using CO.CDP.Common;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Moq;
using Xunit;
using System.ComponentModel.DataAnnotations;
using FluentAssertions;

public class NotEmptyAttributeTests
{
    private readonly NotEmptyAttribute _attribute;
    private readonly Mock<ModelMetadata> _mockMetadata;

    public NotEmptyAttributeTests()
    {
        _attribute = new NotEmptyAttribute { ErrorMessage = "The collection cannot be empty." };
        _mockMetadata = new Mock<ModelMetadata>(MockBehavior.Strict, ModelMetadataIdentity.ForType(typeof(List<string>)));
    }

    [Fact]
    public void Validate_WithNonEmptyList_ShouldReturnNoValidationErrors()
    {
        var context = new ModelValidationContext(
            new ActionContext(),
            _mockMetadata.Object,
            _mockMetadata.Object, null,
            new List<string> { "item1" });

        var result = _attribute.Validate(context);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithEmptyList_ShouldReturnValidationError()
    {
        var context = new ModelValidationContext(
            new ActionContext(),
            _mockMetadata.Object,
            _mockMetadata.Object,
            null,
            new List<string>());

        var result = _attribute.Validate(context);

        result.Single().Should().NotBeNull()
                  .And.BeOfType<ModelValidationResult>()
                  .Which.Message.Should().Be("The collection cannot be empty.");
    }

    [Fact]
    public void Validate_WithNullModel_ShouldReturnValidationError()
    {
        var context = new ModelValidationContext(
            new ActionContext(),
            _mockMetadata.Object,
            _mockMetadata.Object,
            null,
            null);

        // Act
        var result = _attribute.Validate(context);

        result.Single().Should().NotBeNull()
                  .And.BeOfType<ModelValidationResult>()
                  .Which.Message.Should().Be("The collection cannot be empty.");
    }

    [Fact]
    public void Validate_WithNonListModel_ShouldReturnValidationError()
    {
        var context = new ModelValidationContext(
            new ActionContext(),
            _mockMetadata.Object,
            _mockMetadata.Object,
            null,
            new object());

        var result = _attribute.Validate(context);

        result.Single().Should().NotBeNull()
                  .And.BeOfType<ModelValidationResult>()
                  .Which.Message.Should().Be("The collection cannot be empty.");
    }
}