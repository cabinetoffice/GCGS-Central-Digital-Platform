using CO.CDP.OrganisationApp.Pages.ShareInformation;
using FluentAssertions;


namespace CO.CDP.OrganisationApp.Tests.Pages.ShareInformation;
public class ShareCodeConfirmationTests
{
    [Fact]
    public void ShareCodeConfirmationModel_ShouldInitializePropertiesCorrectly()
    {
        var organisationId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var shareCode = "HDJ2123F";

        var model = new ShareCodeConfirmationModel
        {
            OrganisationId = organisationId,
            FormId = formId,
            SectionId = sectionId,
            ShareCode = shareCode
        };

        model.OrganisationId.Should().Be(organisationId);
        model.FormId.Should().Be(formId);
        model.SectionId.Should().Be(sectionId);
        model.ShareCode.Should().Be(shareCode);
    }
}