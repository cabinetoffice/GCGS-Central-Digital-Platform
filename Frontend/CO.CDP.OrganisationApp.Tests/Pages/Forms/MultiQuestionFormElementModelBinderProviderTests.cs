using CO.CDP.OrganisationApp.Pages.Forms;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Forms;

public class MultiQuestionFormElementModelBinderProviderTests
{
    private readonly MultiQuestionFormElementModelBinderProvider _provider = new();
    private readonly EmptyModelMetadataProvider _metadataProvider = new();

    [Fact]
    public void GetBinder_ReturnsBinderForFormElementMultiQuestionModel()
    {
        var mockContext = new Mock<ModelBinderProviderContext>();
        var metadata = _metadataProvider.GetMetadataForType(typeof(FormElementMultiQuestionModel));
        mockContext.Setup(c => c.Metadata).Returns(metadata);

        var result = _provider.GetBinder(mockContext.Object);

        Assert.NotNull(result);
        Assert.IsType<BinderTypeModelBinder>(result);
    }

    [Fact]
    public void GetBinder_ReturnsNullForOtherModelTypes()
    {
        var mockContext = new Mock<ModelBinderProviderContext>();
        var metadata = _metadataProvider.GetMetadataForType(typeof(string));
        mockContext.Setup(c => c.Metadata).Returns(metadata);

        var result = _provider.GetBinder(mockContext.Object);

        Assert.Null(result);
    }

    [Fact]
    public void GetBinder_ThrowsArgumentNullExceptionForNullContext()
    {
        Assert.Throws<ArgumentNullException>(() => _provider.GetBinder(null!));
    }
}