using CO.CDP.OrganisationApp.Pages.Registration;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Registration
{
    public class OrganisationIdentificationModelTests
    {
        private readonly Mock<ILogger<OrganisationIdentificationModel>> _loggerMock;
        private readonly Mock<ISession> _sessionMock;
        private readonly OrganisationIdentificationModel _pageModel;

        public OrganisationIdentificationModelTests()
        {
            _loggerMock = new Mock<ILogger<OrganisationIdentificationModel>>();
            _sessionMock = new Mock<ISession>();
            _pageModel = new OrganisationIdentificationModel(_loggerMock.Object, _sessionMock.Object);
        }

        [Fact]
        public void ValidOrganisationNumber_ShouldStoreInSession_AndRedirect()
        {
            // Arrange
            var orgId = "companiesHouseNumber";
            var validNumber = "123456";
            var organisationNumbers = new Dictionary<string, string> { { orgId, validNumber } };

            // Act
            var result = _pageModel.OnPost(orgId, organisationNumbers);

            // Assert
            _sessionMock.Verify(s => s.Set(orgId, validNumber), Times.Once);
            result.Should().BeOfType<RedirectToPageResult>();
            ((RedirectToPageResult)result).PageName.Should().Be("./OrganisationRegisteredAddress");
        }

        [Fact]
        public void InvalidOrganisationNumber_ShouldSetError_AndReturnPage()
        {
            // Arrange
            var orgId = "companiesHouseNumber";
            var invalidNumber = "";
            var organisationNumbers = new Dictionary<string, string> { { orgId, invalidNumber } };

            // Act
            var result = _pageModel.OnPost(orgId, organisationNumbers);

            // Assert
            _pageModel.HasError.Should().BeTrue();
            result.Should().BeOfType<PageResult>();
        }

        [Fact]
        public void OtherNoneIdentification_ShouldRedirect_WithoutSessionStorage()
        {
            // Arrange
            var orgId = "otherNone";
            var organisationNumbers = new Dictionary<string, string>();

            // Act
            var result = _pageModel.OnPost(orgId, organisationNumbers);

            // Assert
            _sessionMock.Verify(s => s.Set(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            result.Should().BeOfType<RedirectToPageResult>();
            ((RedirectToPageResult)result).PageName.Should().Be("./OrganisationRegisteredAddress");
        }

        [Theory]
        [InlineData("companiesHouseNumber", "122")]
        public void OnPost_ValidOrganisationIdentification_ShouldNotSetHasError(string organisationId, string number)
        {
            // Arrange
            var organisationNumbers = new Dictionary<string, string> { { organisationId, number } };

            // Act
            var result = _pageModel.OnPost(organisationId, organisationNumbers);

            // Assert
            _pageModel.HasError.Should().BeFalse();
            result.Should().BeOfType<RedirectToPageResult>();
        }

        [Theory]
        [InlineData("companiesHouseNumber", "")]
        public void OnPost_InvalidOrganisationIdentification_ShouldSetHasErrorTrue(string organisationId, string number)
        {
            // Arrange
            var organisationNumbers = new Dictionary<string, string> { { organisationId, number } };

            // Act
            var result = _pageModel.OnPost(organisationId, organisationNumbers);

            // Assert
            _pageModel.HasError.Should().BeTrue();
            result.Should().BeOfType<PageResult>();
        }

        [Fact]
        public void OnPost_ValidOrganisationIdentification_ShouldRedirectToOrganisationRegisteredAddress()
        {
            // Arrange
            var organisationNumbers = new Dictionary<string, string>
            {
                { "companiesHouseNumber", "123" }
            };

            // Act
            var result = _pageModel.OnPost("companiesHouseNumber", organisationNumbers);

            // Assert
            result.Should().BeOfType<RedirectToPageResult>();
        }

        [Fact]
        public void OnPost_OtherNone_ShouldRedirectToOrganisationRegisteredAddress()
        {
            var result = _pageModel.OnPost("otherNone", new Dictionary<string, string>());

            // Assert
            result.Should().BeOfType<RedirectToPageResult>();
        }
    }
}
