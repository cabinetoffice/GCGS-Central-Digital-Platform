using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System.Text.Json;

namespace CO.CDP.OrganisationApp.Tests;

public class TempDataServiceTest
{
    private readonly TempDataDictionary _tempData;
    private readonly TempDataService _tempDataService;

    public TempDataServiceTest()
    {
        _tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
        _tempDataService = new TempDataService(_tempData);
    }

    [Fact]
    public void Put_StoresValueInTempData()
    {
        var expectedValue = new TestObject { Name = "John", Age = 30 };

        _tempDataService.Put("testKey", expectedValue);

        _tempData["testKey"].Should().NotBeNull()
            .And.Be(JsonSerializer.Serialize(expectedValue));
    }

    [Fact]
    public void Get_RetrievesStoredValue()
    {
        var expectedValue = new TestObject { Name = "Jane", Age = 25 };
        _tempDataService.Put("testKey", expectedValue);

        var retrievedValue = _tempDataService.Get<TestObject>("testKey");

        retrievedValue.Should().NotBeNull();
        retrievedValue.Should().BeEquivalentTo(expectedValue);
    }

    [Fact]
    public void Get_ReturnsNullWhenKeyNotFound()
    {
        var retrievedValue = _tempDataService.Get<TestObject>("testKey");

        retrievedValue.Should().BeNull();
    }

    [Fact]
    public void GetOrDefault_ReturnsDefaultValueWhenKeyNotFound()
    {
        var defaultValue = _tempDataService.GetOrDefault<TestObject>("nonexistentKey");

        defaultValue.Should().NotBeNull();
        defaultValue.Should().BeEquivalentTo(new TestObject());
    }

    [Fact]
    public void GetOrDefault_ReturnsValueWhenKeyFound()
    {
        var expectedValue = new TestObject { Name = "Jack", Age = 40 };
        _tempDataService.Put("testKey", expectedValue);

        var defaultValue = _tempDataService.GetOrDefault<TestObject>("testKey");

        defaultValue.Should().NotBeNull();
        defaultValue.Should().BeEquivalentTo(expectedValue);
    }

    [Fact]
    public void Peek_ReturnsValueWithoutRemovingIt()
    {
        var expectedValue = new TestObject { Name = "Jack", Age = 40 };
        _tempDataService.Put("testKey", expectedValue);

        var peekedValue = _tempDataService.Peek<TestObject>("testKey");

        peekedValue.Should().NotBeNull();
        peekedValue.Should().BeEquivalentTo(expectedValue);
        _tempData["testKey"].Should().NotBeNull();
    }

    [Fact]
    public void Peek_ReturnsNullWhenKeyNotFound()
    {
        var retrievedValue = _tempDataService.Peek<TestObject>("testKey");

        retrievedValue.Should().BeNull();
    }

    [Fact]
    public void PeekOrDefault_ReturnsDefaultValueWhenKeyNotFound()
    {
        var defaultValue = _tempDataService.PeekOrDefault<TestObject>("nonexistentKey");

        defaultValue.Should().NotBeNull();
        defaultValue.Should().BeEquivalentTo(new TestObject());
    }

    [Fact]
    public void PeekOrDefault_ReturnsValueWhenKeyFound()
    {
        var expectedValue = new TestObject { Name = "Jack", Age = 40 };
        _tempDataService.Put("testKey", expectedValue);

        var defaultValue = _tempDataService.PeekOrDefault<TestObject>("testKey");

        defaultValue.Should().NotBeNull();
        defaultValue.Should().BeEquivalentTo(expectedValue);
        _tempData["testKey"].Should().NotBeNull();
    }

    [Fact]
    public void Remove_RemovesValueFromTempData()
    {
        var expectedValue = new TestObject { Name = "Jill", Age = 35 };
        _tempDataService.Put("testKey", expectedValue);

        _tempDataService.Remove("testKey");

        _tempData["testKey"].Should().BeNull();
    }
}

public class TestObject
{
    public string? Name { get; set; }
    public int Age { get; set; }
}