using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using OrganisationWebApiClient = CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier;

public class LegalFormFormationDateTest
{
    private readonly Mock<ITempDataService> _mockTempDataService;
    private readonly LegalFormFormationDateModel _model;
    private readonly Mock<OrganisationWebApiClient.IOrganisationClient> _mockOrganisationClient;

    public LegalFormFormationDateTest()
    {
        _mockTempDataService = new Mock<ITempDataService>();
        _mockOrganisationClient = new Mock<OrganisationWebApiClient.IOrganisationClient>();
        _model = new LegalFormFormationDateModel(_mockTempDataService.Object, _mockOrganisationClient.Object);
    }

    [Fact]
    public async Task OnGet_OrganisationNotFound_ShouldRedirectToPageNotFound()
    {
        _model.Id = Guid.NewGuid();

        _mockOrganisationClient.Setup(client => client.GetOrganisationAsync(_model.Id))
            .ThrowsAsync(new OrganisationWebApiClient.ApiException("Unexpected error", 404, "", default, null));

        var result = await _model.OnGet();

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnGet_ShouldPopulateFields_WhenDateAwardedIsPresent()
    {
        var dateAwarded = new DateTime(2023, 6, 15);
        var legalForm = new LegalForm { RegistrationDate = dateAwarded };

        _mockOrganisationClient.Setup(o => o.GetOrganisationAsync(_model.Id))
            .ReturnsAsync(GivenOrganisationClientModel(_model.Id));

        _mockTempDataService.Setup(t => t.PeekOrDefault<LegalForm>(LegalForm.TempDataKey)).Returns(legalForm);

        var result = await _model.OnGet();

        _model.Day.Should().Be("15");
        _model.Month.Should().Be("6");
        _model.Year.Should().Be("2023");
        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnGet_ShouldNotPopulateFields_WhenDateAwardedIsNotPresent()
    {
        var legalForm = new LegalForm();
        _mockTempDataService.Setup(t => t.PeekOrDefault<LegalForm>(LegalForm.TempDataKey)).Returns(legalForm);

        var result = await _model.OnGet();

        _model.Day.Should().BeNull();
        _model.Month.Should().BeNull();
        _model.Year.Should().BeNull();
        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnPost_ShouldReturnPage_WhenDateIsInTheFuture()
    {
        SetFutureDate();

        var result = await _model.OnPost();

        _model.ModelState.IsValid.Should().BeFalse();
        _model.ModelState.Should().ContainKey(nameof(_model.RegistrationDate));
        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnPost_ShouldReturnPage_WhenModelStateIsInvalid()
    {
        _model.Day = "40";
        _model.Month = "14";
        _model.Year = "2023";
        _model.ModelState.AddModelError("Day", "Invalid day");

        var result = await _model.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnPost_ShouldReturnPage_WhenDateIsInvalid()
    {
        SetDateFields("31", "2", "2023");

        var result = await _model.OnPost();

        _model.ModelState.IsValid.Should().BeFalse();
        _model.ModelState.Should().ContainKey(nameof(_model.RegistrationDate));
        result.Should().BeOfType<PageResult>();
    }


    [Theory]
    [InlineData("31", "02", "2021", "Date of registration must be a real date")]
    [InlineData("01", "01", "2100", "Date of registration must be today or in the past")]
    public async Task OnPost_AddsModelError_WhenDateIsInvalid(string day, string month, string year, string expectedError)
    {
        SetDateFields(day, month, year);

        var result = await _model.OnPost();

        _model.ModelState.ContainsKey(nameof(_model.RegistrationDate)).Should().BeTrue();
        _model.ModelState[nameof(_model.RegistrationDate)]?.Errors[0].ErrorMessage.Should().Be(expectedError);
        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnPost_UpdatesLegalForm_WhenDateIsValid()
    {
        var validDate = new DateTimeOffset(2023, 6, 15, 0, 0, 0, TimeSpan.FromHours(0));
        SetDateFields(validDate.Day.ToString("D2"), validDate.Month.ToString("D2"), validDate.Year.ToString());
        _model.Id = Guid.NewGuid();

        var legalForm = new LegalForm()
        {
            LawRegistered = "law",
            RegisteredLegalForm = "legal form",
            RegistrationDate = new DateTimeOffset(2023, 6, 15, 0, 0, 0, TimeSpan.FromHours(0)),
            RegisteredUnderAct2006 = true
        };
        _mockTempDataService.Setup(s => s.GetOrDefault<LegalForm>(LegalForm.TempDataKey)).Returns(legalForm);

        var result = await _model.OnPost();

        legalForm.RegistrationDate.Should().Be(validDate);
        _mockTempDataService.Verify(s => s.Put(LegalForm.TempDataKey, legalForm), Times.Once);
        _mockOrganisationClient.Verify(x => x.UpdateSupplierInformationAsync(_model.Id, It.IsAny<OrganisationWebApiClient.UpdateSupplierInformation>()), Times.Once);
        result.Should().BeOfType<RedirectToPageResult>().Which.PageName.Should().Be("SupplierBasicInformation");
    }

    [Fact]
    public async Task OnPost_RedirectsToPageNotFound_WhenApiExceptionIsThrown()
    {
        var validDate = new DateTimeOffset(2023, 6, 15, 0, 0, 0, TimeSpan.FromHours(0));
        SetDateFields(validDate.Day.ToString("D2"), validDate.Month.ToString("D2"), validDate.Year.ToString());
        _model.Id = Guid.NewGuid();

        var legalForm = new LegalForm() { RegisteredUnderAct2006 = false };
        _mockTempDataService.Setup(s => s.GetOrDefault<LegalForm>(LegalForm.TempDataKey)).Returns(legalForm);
        _mockOrganisationClient.Setup(c => c.UpdateSupplierInformationAsync(It.IsAny<Guid>(), It.IsAny<OrganisationWebApiClient.UpdateSupplierInformation>()))
                               .ThrowsAsync(new OrganisationWebApiClient.ApiException("Not Found", 404, null, null, null));

        var result = await _model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>().Which.PageName.Should().Be("LegalFormCompanyActQuestion");
    }

    [Fact]
    public async Task OnPost_ShouldRedirect_WhenDateIsValid()
    {
        SetValidDate();
        _model.Id = Guid.NewGuid();
        var legalForm = new LegalForm()
        {
            LawRegistered = "law",
            RegisteredLegalForm = "legal form",
            RegistrationDate = new DateTimeOffset(2023, 6, 15, 0, 0, 0, TimeSpan.FromHours(0)),
            RegisteredUnderAct2006 = true
        };
        _mockTempDataService.Setup(t => t.GetOrDefault<LegalForm>(LegalForm.TempDataKey)).Returns(legalForm);

        var result = await _model.OnPost();

        _mockTempDataService.Verify(t => t.Put(LegalForm.TempDataKey, It.Is<LegalForm>(ta => ta.RegistrationDate == new DateTimeOffset(2023, 6, 15, 0, 0, 0, TimeSpan.FromHours(0)))), Times.Once);
        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("SupplierBasicInformation");
    }

    private void SetDateFields(string day, string month, string year)
    {
        _model.Day = day;
        _model.Month = month;
        _model.Year = year;
    }

    private void SetValidDate()
    {
        _model.Day = "15";
        _model.Month = "6";
        _model.Year = "2023";
    }

    private void SetFutureDate()
    {
        var futureDate = DateTime.Now.AddDays(1);
        _model.Day = futureDate.Day.ToString();
        _model.Month = futureDate.Month.ToString();
        _model.Year = futureDate.Year.ToString();
    }

    private static OrganisationWebApiClient.Organisation GivenOrganisationClientModel(Guid? id)
    {
        return new OrganisationWebApiClient.Organisation(null, null, null, id!.Value, null, "Test Org", []);
    }
}