using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Registration;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using System.ComponentModel.DataAnnotations;
using OrganisationType = CO.CDP.OrganisationApp.Constants.OrganisationType;

namespace CO.CDP.OrganisationApp.Tests.Pages.Registration;

public class SupplierOrganisationTypeModelTest
{
    private readonly Mock<ISession> _sessionMock;
    private readonly RegistrationDetails _registrationDetails;

    public SupplierOrganisationTypeModelTest()
    {
        _sessionMock = new Mock<ISession>();
        _registrationDetails = new RegistrationDetails
        {
            OrganisationType = OrganisationType.Supplier,
            OrganisationScheme = "GB-COH",
            OrganisationName = "Test Org",
            OrganisationEmailAddress = "test@example.com",
            OrganisationAddressLine1 = "123 Test Street",
            OrganisationCityOrTown = "London",
            OrganisationPostcode = "SW1A 1AA",
            OrganisationCountryCode = "GB"
        };
        _sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey))
                    .Returns(_registrationDetails);
        _sessionMock.Setup(session => session.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails { UserUrn = "urn:test" });
    }

    [Fact]
    public void OnGet_WhenNoOperationTypesSelected_ReturnsPageResultWithNullSelection()
    {
        _registrationDetails.SupplierOrganisationOperationTypes = [];

        var model = new SupplierOrganisationTypeModel(_sessionMock.Object);

        var result = model.OnGet();

        result.Should().BeOfType<PageResult>();
        model.SelectedOperationTypes.Should().BeNull();
    }

    [Fact]
    public void OnGet_WhenOperationTypesAlreadySelected_ReturnsPageResultWithExistingSelection()
    {
        _registrationDetails.SupplierOrganisationOperationTypes = [OperationType.SmallOrMediumSized, OperationType.NonGovernmental];

        var model = new SupplierOrganisationTypeModel(_sessionMock.Object);

        var result = model.OnGet();

        result.Should().BeOfType<PageResult>();
        model.SelectedOperationTypes.Should().NotBeNull();
        model.SelectedOperationTypes.Should().HaveCount(2);
        model.SelectedOperationTypes.Should().Contain(OperationType.SmallOrMediumSized);
        model.SelectedOperationTypes.Should().Contain(OperationType.NonGovernmental);
    }

    [Fact]
    public void OnPost_WhenValidModelState_SavesOperationTypesToSessionAndRedirectsToSummary()
    {
        var model = new SupplierOrganisationTypeModel(_sessionMock.Object)
        {
            SelectedOperationTypes = [OperationType.SmallOrMediumSized]
        };

        var result = model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("OrganisationDetailsSummary");

        _sessionMock.Verify(s => s.Set(Session.RegistrationDetailsKey, It.Is<RegistrationDetails>(
            r => r.SupplierOrganisationOperationTypes.Count == 1 &&
                 r.SupplierOrganisationOperationTypes.Contains(OperationType.SmallOrMediumSized)
        )), Times.Once);
    }

    [Fact]
    public void OnPost_WhenRedirectToSummaryIsTrue_RedirectsToOrganisationDetailsSummary()
    {
        var model = new SupplierOrganisationTypeModel(_sessionMock.Object)
        {
            SelectedOperationTypes = [OperationType.PublicService],
            RedirectToSummary = true
        };

        var result = model.OnPost();

        var redirectResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectResult.PageName.Should().Be("OrganisationDetailsSummary");
    }

    [Fact]
    public void OnPost_WhenRedirectToSummaryIsFalse_RedirectsToOrganisationDetailsSummary()
    {
        var model = new SupplierOrganisationTypeModel(_sessionMock.Object)
        {
            SelectedOperationTypes = [OperationType.SupportedEmploymentProvider],
            RedirectToSummary = false
        };

        var result = model.OnPost();

        var redirectResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectResult.PageName.Should().Be("OrganisationDetailsSummary");
    }

    [Fact]
    public void OnPost_WhenModelStateIsInvalid_ReturnsPage()
    {
        var model = new SupplierOrganisationTypeModel(_sessionMock.Object)
        {
            SelectedOperationTypes = null
        };
        model.ModelState.AddModelError("SelectedOperationTypes", "Please select at least one operation type");

        var result = model.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void OnPost_WhenMultipleOperationTypesSelected_SavesAllToSession()
    {
        var selectedTypes = new List<OperationType>
        {
            OperationType.SmallOrMediumSized,
            OperationType.NonGovernmental,
            OperationType.SupportedEmploymentProvider
        };

        var model = new SupplierOrganisationTypeModel(_sessionMock.Object)
        {
            SelectedOperationTypes = selectedTypes
        };

        var result = model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>();

        _sessionMock.Verify(s => s.Set(Session.RegistrationDetailsKey, It.Is<RegistrationDetails>(
            r => r.SupplierOrganisationOperationTypes.Count == 3
        )), Times.Once);
    }

    [Fact]
    public void OnPost_WhenNoneOperationTypeSelected_SavesNoneToSession()
    {
        var model = new SupplierOrganisationTypeModel(_sessionMock.Object)
        {
            SelectedOperationTypes = [OperationType.None]
        };

        var result = model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>();

        _sessionMock.Verify(s => s.Set(Session.RegistrationDetailsKey, It.Is<RegistrationDetails>(
            r => r.SupplierOrganisationOperationTypes.Count == 1 &&
                 r.SupplierOrganisationOperationTypes.Contains(OperationType.None)
        )), Times.Once);
    }

    [Fact]
    public void OnPost_WhenSelectedOperationTypesIsNull_SavesEmptyListToSession()
    {
        var model = new SupplierOrganisationTypeModel(_sessionMock.Object)
        {
            SelectedOperationTypes = null
        };
        // Clear ModelState to simulate valid state
        model.ModelState.Clear();

        var result = model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>();

        _sessionMock.Verify(s => s.Set(Session.RegistrationDetailsKey, It.Is<RegistrationDetails>(
            r => r.SupplierOrganisationOperationTypes.Count == 0
        )), Times.Once);
    }

    [Fact]
    public void ValidateModel_WithNoneAndOtherOperationTypes_ShouldReturnValidationError()
    {
        var model = new SupplierOrganisationTypeModel(_sessionMock.Object)
        {
            SelectedOperationTypes =
            [
                OperationType.None,
                OperationType.SmallOrMediumSized
            ]
        };

        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model);

        var isValid = Validator.TryValidateObject(model, validationContext, validationResults, true);

        isValid.Should().BeFalse();
        validationResults.Should().ContainSingle();
        validationResults[0].ErrorMessage.Should().Be("You cannot select 'None apply' along with other options.");
    }

    [Fact]
    public void ValidateModel_WithOnlyNoneOperationType_ShouldBeValid()
    {
        var model = new SupplierOrganisationTypeModel(_sessionMock.Object)
        {
            SelectedOperationTypes = [OperationType.None]
        };

        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model);

        var isValid = Validator.TryValidateObject(model, validationContext, validationResults, true);

        isValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateModel_WithMultipleOperationTypesExcludingNone_ShouldBeValid()
    {
        var model = new SupplierOrganisationTypeModel(_sessionMock.Object)
        {
            SelectedOperationTypes =
            [
                OperationType.SmallOrMediumSized,
                OperationType.NonGovernmental,
                OperationType.PublicService
            ]
        };

        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model);

        var isValid = Validator.TryValidateObject(model, validationContext, validationResults, true);

        isValid.Should().BeTrue();
    }

    [Fact]
    public void CurrentPage_ShouldReturnSupplierOrganisationTypePage()
    {
        var model = new SupplierOrganisationTypeModel(_sessionMock.Object);

        model.CurrentPage.Should().Be(RegistrationStepModel.SupplierOrganisationTypePage);
    }
}
