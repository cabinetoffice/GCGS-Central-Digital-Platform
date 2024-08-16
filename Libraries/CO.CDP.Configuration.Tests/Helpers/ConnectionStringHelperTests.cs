using Moq;
using Microsoft.Extensions.Configuration;
using CO.CDP.Configuration.Helpers;
using Xunit;

namespace CO.CDP.Configuration.Tests.Helpers;
public class ConnectionStringHelperTests
{
    [Fact]
    public void GetConnectionString_ReturnsCorrectConnectionString_WhenAllValuesAreProvided()
    {
        // Arrange
        var mockConfiguration = new Mock<IConfiguration>();
        var connectionSection = new Mock<IConfigurationSection>();

        connectionSection.Setup(x => x["Server"]).Returns("localhost");
        connectionSection.Setup(x => x["Database"]).Returns("cdp");
        connectionSection.Setup(x => x["Username"]).Returns("cdp_user");
        connectionSection.Setup(x => x["Password"]).Returns("password");

        mockConfiguration.Setup(x => x.GetSection("OrganisationInformationDatabase"))
                         .Returns(connectionSection.Object);

        // Act
        var connectionString = ConnectionStringHelper.GetConnectionString(mockConfiguration.Object, "OrganisationInformationDatabase");

        // Assert
        Assert.Equal("Server=localhost;Database=cdp;Username=cdp_user;Password=password;", connectionString);
    }

    [Fact]
    public void GetConnectionString_ThrowsArgumentNullException_WhenConfigurationIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            ConnectionStringHelper.GetConnectionString(null, "OrganisationInformationDatabase"));
    }

    [Fact]
    public void GetConnectionString_ThrowsInvalidOperationException_WhenSectionIsMissing()
    {
        // Arrange
        var mockConfiguration = new Mock<IConfiguration>();

        mockConfiguration.Setup(x => x.GetSection("NonExistentSection")).Returns((IConfigurationSection?)null);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            ConnectionStringHelper.GetConnectionString(mockConfiguration.Object, "NonExistentSection"));
    }

    [Fact]
    public void GetConnectionString_ThrowsInvalidOperationException_WhenPasswordIsMissing()
    {
        // Arrange
        var mockConfiguration = new Mock<IConfiguration>();
        var connectionSection = new Mock<IConfigurationSection>();

        connectionSection.Setup(x => x["Host"]).Returns("localhost");
        connectionSection.Setup(x => x["Database"]).Returns("cdp");
        connectionSection.Setup(x => x["Username"]).Returns("cdp_user");

        mockConfiguration.Setup(x => x.GetSection("OrganisationInformationDatabase")).Returns(connectionSection.Object);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            ConnectionStringHelper.GetConnectionString(mockConfiguration.Object, "OrganisationInformationDatabase"));
    }
}
