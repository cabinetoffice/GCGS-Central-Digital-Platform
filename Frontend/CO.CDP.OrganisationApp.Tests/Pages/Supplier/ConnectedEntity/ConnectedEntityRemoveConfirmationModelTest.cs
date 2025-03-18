using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Supplier.ConnectedEntity;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier.ConnectedEntity;

public class ConnectedEntityRemoveConfirmationModelTest
{
    private readonly Mock<IOrganisationClient> _organisationClientMock;
    private readonly ConnectedEntityRemoveConfirmationModel _model;

    public ConnectedEntityRemoveConfirmationModelTest()
    {
        _organisationClientMock = new Mock<IOrganisationClient>();
        _model = new ConnectedEntityRemoveConfirmationModel(_organisationClientMock.Object)
        {
            Id = Guid.NewGuid(),
            ConnectedPersonId = Guid.NewGuid()
        };
    }

    [Fact]
    public async Task OnGet_ShouldRedirect_WhenConnectedEntityNotFound()
    {
        _organisationClientMock.Setup(c => c.GetConnectedEntitiesAsync(It.IsAny<Guid>()))
            .ReturnsAsync([]);

        var result = await _model.OnGet();

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnGet_ShouldReturnPage_WhenConnectedEntityFound()
    {
        _model.ConnectedPersonId = Guid.NewGuid();

        _organisationClientMock.Setup(c => c.GetConnectedEntitiesAsync(It.IsAny<Guid>()))
            .ReturnsAsync([new ConnectedEntityLookup (entityId : _model.ConnectedPersonId,
            entityType : ConnectedEntityType.Individual,
            name : "connected",
            uri : null)]);

        var result = await _model.OnGet();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnPost_ShouldReturnPage_WhenModelStateIsInvalid()
    {
        _model.ModelState.AddModelError("TestError", "Invalid Model");

        var result = await _model.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnPost_ShouldCallDeleteConnectedEntityAsync_WhenValid()
    {
        _model.ConfirmRemove = RemoveConnectedPersonReason.AddedInError;
        _organisationClientMock.Setup(c => c.DeleteConnectedEntityAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DeleteConnectedEntity>()))
            .Returns(Task.CompletedTask);

        var result = await _model.OnPost();

        _organisationClientMock.Verify(c => c.DeleteConnectedEntityAsync(_model.Id, _model.ConnectedPersonId, It.IsAny<DeleteConnectedEntity>()), Times.Once);
        result.Should().BeOfType<RedirectToPageResult>();
    }

    [Fact]
    public async Task OnPost_ShouldRedirectToSummaryPage_OnSuccess()
    {
        _model.ConfirmRemove = RemoveConnectedPersonReason.AddedInError;

        var result = await _model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("ConnectedPersonSummary");
    }
}