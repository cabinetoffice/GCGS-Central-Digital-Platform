using CO.CDP.UI.Foundation.Session;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Text;
using System.Text.Json;
using Xunit;

namespace CO.CDP.UI.Foundation.Tests.Session;

public class SessionServiceTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<ISession> _sessionMock;
    private readonly SessionService _sessionService;

    public SessionServiceTests()
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _sessionMock = new Mock<ISession>();

        var httpContext = new DefaultHttpContext();
        httpContext.Session = _sessionMock.Object;

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        _sessionService = new SessionService(_httpContextAccessorMock.Object);
    }

    [Fact]
    public void Constructor_WhenHttpContextAccessorIsNull_ThrowsArgumentNullException()
    {
        Action act = () => new SessionService(null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("httpContextAccessor");
    }

    [Fact]
    public void GetValue_WhenKeyIsValid_ReturnsValue()
    {
        var testKey = "testKey";
        var testValue = new { Name = "Test" };
        var serializedValue = JsonSerializer.Serialize(testValue);
        var bytes = Encoding.UTF8.GetBytes(serializedValue);
        _sessionMock.Setup(s => s.TryGetValue(testKey, out bytes)).Returns(true);

        var result = _sessionService.GetValue<object>(testKey);

        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void GetValue_WhenKeyIsNullOrEmpty_ThrowsArgumentException(string key)
    {
        Action act = () => _sessionService.GetValue<object>(key);
        act.Should().Throw<ArgumentException>().WithParameterName("key");
    }

    [Fact]
    public void SetValue_WhenKeyAndValueAreValid_SetsValueInSession()
    {
        var testKey = "testKey";
        var testValue = "testValue";

        _sessionService.SetValue(testKey, testValue);

        _sessionMock.Verify(s => s.Set(testKey, It.IsAny<byte[]>()), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void SetValue_WhenKeyIsNullOrEmpty_ThrowsArgumentException(string key)
    {
        Action act = () => _sessionService.SetValue(key, "value");
        act.Should().Throw<ArgumentException>().WithParameterName("key");
    }

    [Fact]
    public void RemoveValue_WhenKeyIsValid_RemovesValueFromSession()
    {
        var testKey = "testKey";

        _sessionService.RemoveValue(testKey);

        _sessionMock.Verify(s => s.Remove(testKey), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void RemoveValue_WhenKeyIsNullOrEmpty_ThrowsArgumentException(string key)
    {
        Action act = () => _sessionService.RemoveValue(key);
        act.Should().Throw<ArgumentException>().WithParameterName("key");
    }

    [Fact]
    public void HasKey_WhenKeyExists_ReturnsTrue()
    {
        var testKey = "testKey";
        _sessionMock.Setup(s => s.Keys).Returns([testKey]);

        var result = _sessionService.HasKey(testKey);

        result.Should().BeTrue();
    }

    [Fact]
    public void HasKey_WhenKeyDoesNotExist_ReturnsFalse()
    {
        var testKey = "testKey";
        _sessionMock.Setup(s => s.Keys).Returns([]);

        var result = _sessionService.HasKey(testKey);

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void HasKey_WhenKeyIsNullOrEmpty_ThrowsArgumentException(string key)
    {
        Action act = () => _sessionService.HasKey(key);
        act.Should().Throw<ArgumentException>().WithParameterName("key");
    }

    [Fact]
    public void GetString_WhenKeyIsValid_ReturnsString()
    {
        var testKey = "testKey";
        var testValue = "testValue";
        byte[]? bytes = Encoding.UTF8.GetBytes(testValue);
        _sessionMock.Setup(s => s.TryGetValue(testKey, out bytes)).Returns(true);

        var result = _sessionService.GetString(testKey);

        result.Should().Be(testValue);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void GetString_WhenKeyIsNullOrEmpty_ThrowsArgumentException(string key)
    {
        Action act = () => _sessionService.GetString(key);
        act.Should().Throw<ArgumentException>().WithParameterName("key");
    }

    [Fact]
    public void SetString_WhenKeyAndValueAreValid_SetsStringInSession()
    {
        var testKey = "testKey";
        var testValue = "testValue";
        var testValueBytes = Encoding.UTF8.GetBytes(testValue);

        _sessionService.SetString(testKey, testValue);

        _sessionMock.Verify(s => s.Set(testKey, It.Is<byte[]>(v => v.SequenceEqual(testValueBytes))), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void SetString_WhenKeyIsNullOrEmpty_ThrowsArgumentException(string key)
    {
        Action act = () => _sessionService.SetString(key, "value");
        act.Should().Throw<ArgumentException>().WithParameterName("key");
    }

    [Fact]
    public void GetSession_WhenHttpContextIsNull_ThrowsInvalidOperationException()
    {
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext)null!);
        var service = new SessionService(_httpContextAccessorMock.Object);

        Action act = () => service.GetValue<string>("test");

        act.Should().Throw<InvalidOperationException>().WithMessage("HTTP context or session is not available");
    }

    [Fact]
    public void GetSession_WhenSessionIsNull_ThrowsInvalidOperationException()
    {
        var context = new DefaultHttpContext();
        context.Session = null!;
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);
        var service = new SessionService(_httpContextAccessorMock.Object);

        Action act = () => service.GetValue<string>("test");

        act.Should().Throw<InvalidOperationException>().WithMessage("HTTP context or session is not available");
    }
}