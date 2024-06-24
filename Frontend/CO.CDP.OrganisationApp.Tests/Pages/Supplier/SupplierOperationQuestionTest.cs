using System.ComponentModel.DataAnnotations;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier;

public class SupplierOperationQuestionTest
{
    private readonly Mock<ISession> _sessionMock;
    private readonly Mock<IOrganisationClient> _organisationClientMock;
    private readonly SupplierOperationQuestionModel _model;

    public SupplierOperationQuestionTest()
    {
        _sessionMock = new Mock<ISession>();
        _organisationClientMock = new Mock<IOrganisationClient>();
        _model = new SupplierOperationQuestionModel(_sessionMock.Object, _organisationClientMock.Object)
        {
            SelectedOperationTypes = new List<OperationType>()
        };
    }

    [Fact]
    public async Task OnGet_ValidId_ReturnsPageResult()
    {
        var id = Guid.NewGuid();

        _organisationClientMock.Setup(client => client.GetOrganisationSupplierInformationAsync(id))
            .ReturnsAsync(SupplierDetailsFactory.CreateSupplierInformationClientModel());

        _organisationClientMock.Setup(client => client.GetOrganisationAsync(id))
            .ReturnsAsync(SupplierDetailsFactory.GivenOrganisationClientModel(id));

        var result = await _model.OnGet(id);

        result.Should().BeOfType<PageResult>();
        _model.SelectedOperationTypes.Should().NotBeNull();
    }

    [Fact]
    public async Task OnGet_OrganisationNotFound_ReturnsRedirectToPageNotFound()
    {
        var id = Guid.NewGuid();
        _organisationClientMock.Setup(client => client.GetOrganisationSupplierInformationAsync(id))
            .ThrowsAsync(new ApiException("Unexpected error", 404, "", default, null));

        var result = await _model.OnGet(id);

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnPost_ValidModelState_ReturnsRedirectToSupplierBasicInformation()
    {
        var id = Guid.NewGuid();
        _model.Id = id;
        _model.SelectedOperationTypes = new List<OperationType> { OperationType.SmallorMediumSized };

        _organisationClientMock.Setup(client => client.GetOrganisationAsync(id))
            .ReturnsAsync(SupplierDetailsFactory.GivenOrganisationClientModel(id));

        _organisationClientMock.Setup(client => client.UpdateSupplierInformationAsync(id,
            It.IsAny<UpdateSupplierInformation>())).Returns(Task.CompletedTask);

        var result = await _model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("SupplierBasicInformation");
    }

    [Fact]
    public async Task OnPost_InvalidModelState_ReturnsPageResult()
    {
        _model.ModelState.AddModelError("SelectedOperationTypes", "Please select at least one operation type");

        var result = await _model.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void ValidateModel_WithNoneAndOtherOperationTypes_ShouldReturnValidationError()
    {
        var model = new SupplierOperationQuestionModel(_sessionMock.Object, _organisationClientMock.Object)
        {
            SelectedOperationTypes = new List<OperationType>
            {
                OperationType.None,
                OperationType.SmallorMediumSized
            }
        };

        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model);

        var isValid = Validator.TryValidateObject(model, validationContext, validationResults, true);

        isValid.Should().BeFalse();
        validationResults.Should().ContainSingle();
        validationResults[0].ErrorMessage.Should().Be("You cannot select 'My organisation is none of the above' along with other options.");
    }
    [Fact]
    public async Task OnPost_OrganisationNotFound_ReturnsRedirectToPageNotFound()
    {
        var id = Guid.NewGuid();
        _model.Id = id;
        _model.SelectedOperationTypes = new List<OperationType> { OperationType.None };

        _organisationClientMock.Setup(client => client.GetOrganisationAsync(id))
            .ThrowsAsync(new ApiException("Unexpected error", 404, "", default, null));

        var result = await _model.OnPost();

        result.Should().BeOfType<RedirectResult>().Which.Url.Should().Be("/page-not-found");
    }
}