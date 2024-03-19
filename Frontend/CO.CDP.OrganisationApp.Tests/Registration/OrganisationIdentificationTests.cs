using CO.CDP.OrganisationApp.Pages.Registration;
using CO.CDP.OrganisationApp.Tests.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Registration
{
    public class OrganisationIdentificationModelTests
    {
        private readonly OrganisationIdentificationModel _pageModel;
        private readonly Session _session;
        private readonly DefaultHttpContext _httpContext;
        private readonly Mock<ILogger<OrganisationIdentificationModel>> _loggerMock;

        public OrganisationIdentificationModelTests()
        {
            _loggerMock = new Mock<ILogger<OrganisationIdentificationModel>>();

            _httpContext = new DefaultHttpContext
            {
                Session = new MockHttpSession()
            };
            var httpContextAccessor = new HttpContextAccessor { HttpContext = _httpContext };

            _session = new Session(httpContextAccessor);

            _pageModel = new OrganisationIdentificationModel(_loggerMock.Object, _session);
        }

        [Fact]
        public void ValidIdentificationNumber_ShouldStoreInSession()
        {
            // Arrange
            var validOrganisationId = "companiesHouseNumber";
            var validNumber = "12345678";
            var organisationNumbers = new Dictionary<string, string> { { validOrganisationId, validNumber } };

            // Act
            _pageModel.OnPost(validOrganisationId, organisationNumbers);

            // Assert            
            string storedNumber = _session.Get<string>(validOrganisationId);
            storedNumber.Should().Be(validNumber);
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
