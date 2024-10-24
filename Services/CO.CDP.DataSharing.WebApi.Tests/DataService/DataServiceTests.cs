using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.Localization;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Localization;
using Moq;
using static CO.CDP.DataSharing.WebApi.Tests.DataSharingFactory;
using SharedConsent = CO.CDP.OrganisationInformation.Persistence.Forms.SharedConsent;

namespace CO.CDP.DataSharing.WebApi.Tests.DataService;

public class DataServiceTests
{
    private readonly Mock<IShareCodeRepository> _shareCodeRepository = new();
    private readonly Mock<IConnectedEntityRepository> _connectedEntityRepository = new();
    private readonly Mock<IHtmlLocalizer<FormsEngineResource>> _localizer = new();
    private CO.CDP.DataSharing.WebApi.DataService.DataService DataService => new(_shareCodeRepository.Object,
        _connectedEntityRepository.Object, _localizer.Object);

    public DataServiceTests()
    {
        _localizer.Setup(l => l[It.IsAny<string>()])
            .Returns((string key) =>
            {
                if (key == "FinancialInformation_SectionTitle")
                {
                    return new LocalizedHtmlString("FinancialInformation_SectionTitle", "Financial Information");
                }

                return new LocalizedHtmlString(key, key);
            });
    }

    [Fact]
    public async Task GetSharedSupplierInformationAsync_ShouldReturnSharedSupplierInformation_WhenOrganisationExists()
    {
        var shareCode = "ABC-123";
        var sharedConsent = CreateSharedConsent(shareCode: shareCode);
        _shareCodeRepository.Setup(r => r.GetByShareCode(shareCode)).ReturnsAsync(sharedConsent);

        var result = await DataService.GetSharedSupplierInformationAsync(shareCode);

        result.Should().NotBeNull();
        result.BasicInformation.SupplierType.Should().Be(sharedConsent.Organisation.SupplierInfo?.SupplierType);
    }

    [Fact]
    public async Task
        GetSharedSupplierInformationAsync_ShouldThrowSupplierInformationNotFoundException_WhenSupplierInfoIsNull()
    {
        var shareCode = "ABC-123";
        var sharedConsent = CreateSharedConsent(shareCode: shareCode);
        sharedConsent.Organisation.SupplierInfo = null;

        _shareCodeRepository.Setup(r => r.GetByShareCode(shareCode)).ReturnsAsync(sharedConsent);

        Func<Task> act = async () => await DataService.GetSharedSupplierInformationAsync(shareCode);

        await act.Should().ThrowAsync<SupplierInformationNotFoundException>()
            .WithMessage("Supplier information not found.");
    }

    [Fact]
    public async Task ShouldThrowShareCodeNotFoundException_WhenShareCodeDoesNotExist()
    {
        var shareCode = "invalid-sharecode";

        _shareCodeRepository.Setup(repo => repo.GetByShareCode(shareCode))
            .ReturnsAsync((SharedConsent?)null);

        Func<Task> act = async () => await DataService.GetSharedSupplierInformationAsync(shareCode);

        await act.Should().ThrowAsync<ShareCodeNotFoundException>();
    }

    [Fact]
    public async Task ShouldMapOrganisationToBasicInformation()
    {
        var sharedConsent = CreateSharedConsent();
        var organisation = sharedConsent.Organisation;

        _shareCodeRepository.Setup(r => r.GetByShareCode("ABC-123")).ReturnsAsync(sharedConsent);

        var result = await DataService.GetSharedSupplierInformationAsync("ABC-123");

        result.BasicInformation.SupplierType.Should().Be(organisation.SupplierInfo?.SupplierType);
        result.BasicInformation.RegisteredAddress.Should().NotBeNull();
        result.BasicInformation.PostalAddress.Should().NotBeNull();
        result.BasicInformation.VatNumber.Should()
            .Be(organisation.Identifiers.FirstOrDefault(i => i.Scheme == "VAT")?.IdentifierId);
        result.BasicInformation.WebsiteAddress.Should().Be(organisation.ContactPoints.FirstOrDefault()?.Url);
        result.BasicInformation.EmailAddress.Should().Be(organisation.ContactPoints.FirstOrDefault()?.Email);
        result.BasicInformation.LegalForm.Should().NotBeNull();
    }

    [Fact]
    public async Task ShouldMapFormAnswerSetsForPdf()
    {
        var org = CreateOrganisation();
        var sharedConsent = EntityFactory.GetSharedConsent(org.Id, org.Guid, Guid.NewGuid());
        sharedConsent.Organisation = org;

        _shareCodeRepository.Setup(r => r.GetByShareCode("ABC-123")).ReturnsAsync(sharedConsent);

        var result = await DataService.GetSharedSupplierInformationAsync("ABC-123");

        result.FormAnswerSetForPdfs.Should().ContainSingle();

        var exclusionsSection = result.FormAnswerSetForPdfs.First(fa => fa.SectionType == CO.CDP.OrganisationInformation.Persistence.Forms.FormSectionType.Exclusions);
        exclusionsSection.SectionName.Should().Be("Financial Information");
        exclusionsSection.QuestionAnswers.Should().Contain(qa => qa.Item1 == "Were your accounts audited?" && qa.Item2 == "yes");
        exclusionsSection.QuestionAnswers.Should().Contain(qa => qa.Item1 == "What is the financial year end date for the information you uploaded?" && qa.Item2 == DateTime.Today.ToString("dd-MM-yyyy"));
        exclusionsSection.QuestionAnswers.Should().Contain(qa => qa.Item1 == "Upload your accounts" && qa.Item2 == "a_dummy_file.pdf");
    }
}