using CO.CDP.UI.Foundation.Session;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Text;
using System.Text.Json;
using Xunit;

namespace CO.CDP.UI.Foundation.Tests.Session;

public class SessionExtensionsTests
{
    private readonly Mock<ISession> _sessionMock = new();

    private class TestModel
    {
        public string? Name { get; set; }
    }

    [Fact]
    public void Get_WhenDataExists_ReturnsDeserializedObject()
    {
        var testKey = "testKey";
        var testValue = new TestModel { Name = "Test" };
        var serializedValue = JsonSerializer.Serialize(testValue);
        var buffer = Encoding.UTF8.GetBytes(serializedValue);
        _sessionMock.Setup(s => s.TryGetValue(testKey, out buffer)).Returns(true);

        var result = _sessionMock.Object.Get<TestModel>(testKey);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(testValue);
    }

    [Fact]
    public void Get_WhenDataIsNull_ReturnsDefault()
    {
        var testKey = "testKey";
        byte[]? buffer = null;
        _sessionMock.Setup(s => s.TryGetValue(testKey, out buffer)).Returns(true);

        var result = _sessionMock.Object.Get<TestModel>(testKey);

        result.Should().BeNull();
    }

    [Fact]
    public void Get_WhenDataIsEmpty_ReturnsDefault()
    {
        var testKey = "testKey";
        byte[]? buffer = [];
        _sessionMock.Setup(s => s.TryGetValue(testKey, out buffer)).Returns(false);

        var result = _sessionMock.Object.Get<TestModel>(testKey);

        result.Should().BeNull();
    }

    [Fact]
    public void Get_WhenJsonIsInvalid_ReturnsDefault()
    {
        var testKey = "testKey";
        var invalidJson = "invalid-json";
        var buffer = Encoding.UTF8.GetBytes(invalidJson);
        _sessionMock.Setup(s => s.TryGetValue(testKey, out buffer)).Returns(true);

        var result = _sessionMock.Object.Get<TestModel>(testKey);

        result.Should().BeNull();
    }

    [Fact]
    public void Set_SerializesAndSetsValueInSession()
    {
        var testKey = "testKey";
        var testValue = new TestModel { Name = "Test" };
        var serializedValue = JsonSerializer.Serialize(testValue);

        _sessionMock.Object.Set(testKey, testValue);

        _sessionMock.Verify(s => s.Set(testKey, It.Is<byte[]>(b => Encoding.UTF8.GetString(b) == serializedValue)), Times.Once);
    }
}
