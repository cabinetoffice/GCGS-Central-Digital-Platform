using CO.CDP.RegisterOfCommercialTools.App.Models;
using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;
using FluentAssertions;

namespace CO.CDP.RegisterOfCommercialTools.App.Tests.Models;

public class CpvSelectionViewModelTests
{
    [Fact]
    public void IsSelected_WhenCodeIsSelected_ReturnsTrue()
    {
        var viewModel = new CpvSelectionViewModel
        {
            SelectedCodes = ["03000000", "03100000"]
        };

        viewModel.IsSelected("03000000").Should().BeTrue();
        viewModel.IsSelected("03100000").Should().BeTrue();
    }

    [Fact]
    public void IsSelected_WhenCodeIsNotSelected_ReturnsFalse()
    {
        var viewModel = new CpvSelectionViewModel
        {
            SelectedCodes = ["03000000"]
        };

        viewModel.IsSelected("03100000").Should().BeFalse();
        viewModel.IsSelected("99999999").Should().BeFalse();
    }

    [Fact]
    public void SelectionCount_ReturnsCorrectCount()
    {
        var viewModel = new CpvSelectionViewModel
        {
            SelectedCodes = ["03000000", "03100000", "03200000"]
        };

        viewModel.SelectionCount.Should().Be(3);
    }

    [Fact]
    public void SelectionCount_WhenEmpty_ReturnsZero()
    {
        var viewModel = new CpvSelectionViewModel();

        viewModel.SelectionCount.Should().Be(0);
    }

    [Fact]
    public void GetSelectedItems_ReturnsMatchingCodes()
    {
        var allCodes = new List<CpvCodeDto>
        {
            new() { Code = "03000000", DescriptionEn = "Agricultural products", DescriptionCy = "Cynhyrchion amaethyddol", Level = 1 },
            new() { Code = "03100000", DescriptionEn = "Live animals", DescriptionCy = "Anifeiliaid byw", Level = 2 },
            new() { Code = "09000000", DescriptionEn = "Petroleum products", DescriptionCy = "Cynhyrchion petroleum", Level = 1 }
        };

        var viewModel = new CpvSelectionViewModel
        {
            SelectedCodes = ["03000000", "03100000"]
        };

        var result = viewModel.GetSelectedItems(allCodes);

        result.Should().HaveCount(2);
        result.Should().Contain(c => c.Code == "03000000");
        result.Should().Contain(c => c.Code == "03100000");
        result.Should().NotContain(c => c.Code == "09000000");
    }

    [Fact]
    public void GetSelectedItems_WithNestedCodes_ReturnsMatchingCodes()
    {
        var allCodes = new List<CpvCodeDto>
        {
            new()
            {
                Code = "03000000",
                DescriptionEn = "Agricultural products",
                DescriptionCy = "Cynhyrchion amaethyddol",
                Level = 1,
                Children = new List<CpvCodeDto>
                {
                    new() { Code = "03100000", DescriptionEn = "Live animals", DescriptionCy = "Anifeiliaid byw", Level = 2 }
                }
            }
        };

        var viewModel = new CpvSelectionViewModel
        {
            SelectedCodes = ["03100000"]
        };

        var result = viewModel.GetSelectedItems(allCodes);

        result.Should().HaveCount(1);
        result.First().Code.Should().Be("03100000");
    }

    [Fact]
    public void GetSelectedItems_WhenNoMatches_ReturnsEmpty()
    {
        var allCodes = new List<CpvCodeDto>
        {
            new() { Code = "03000000", DescriptionEn = "Agricultural products", DescriptionCy = "Cynhyrchion amaethyddol", Level = 1 }
        };

        var viewModel = new CpvSelectionViewModel
        {
            SelectedCodes = ["99999999"]
        };

        var result = viewModel.GetSelectedItems(allCodes);

        result.Should().BeEmpty();
    }

    [Fact]
    public void ToggleSelection_WhenCodeNotSelected_AddsCode()
    {
        var viewModel = new CpvSelectionViewModel
        {
            SelectedCodes = ["03000000"],
            RootCodes = [new() { Code = "03000000", DescriptionEn = "Agricultural", DescriptionCy = "Amaethyddol", Level = 1 }],
            SearchQuery = "test"
        };

        var result = viewModel.ToggleSelection("03100000");

        result.SelectedCodes.Should().Contain("03000000");
        result.SelectedCodes.Should().Contain("03100000");
        result.SelectedCodes.Should().HaveCount(2);
        result.RootCodes.Should().BeEquivalentTo(viewModel.RootCodes);
        result.SearchQuery.Should().Be("test");
    }

    [Fact]
    public void ToggleSelection_WhenCodeSelected_RemovesCode()
    {
        var viewModel = new CpvSelectionViewModel
        {
            SelectedCodes = ["03000000", "03100000"]
        };

        var result = viewModel.ToggleSelection("03000000");

        result.SelectedCodes.Should().NotContain("03000000");
        result.SelectedCodes.Should().Contain("03100000");
        result.SelectedCodes.Should().HaveCount(1);
    }

    [Fact]
    public void WithSearchResults_CreatesNewInstanceWithSearchData()
    {
        var originalModel = new CpvSelectionViewModel
        {
            SelectedCodes = ["03000000"],
            RootCodes = [new() { Code = "03000000", DescriptionEn = "Agricultural", DescriptionCy = "Amaethyddol", Level = 1 }]
        };

        var searchResults = new List<CpvCodeDto>
        {
            new() { Code = "09000000", DescriptionEn = "Petroleum", DescriptionCy = "Petroleum", Level = 1 }
        };

        var result = originalModel.WithSearchResults(searchResults, "petroleum");

        result.SearchResults.Should().BeEquivalentTo(searchResults);
        result.SearchQuery.Should().Be("petroleum");
        result.SelectedCodes.Should().BeEquivalentTo(originalModel.SelectedCodes);
        result.RootCodes.Should().BeEquivalentTo(originalModel.RootCodes);
        result.IsLoading.Should().BeFalse();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void WithRootCodes_CreatesNewInstanceWithRootData()
    {
        var originalModel = new CpvSelectionViewModel
        {
            SelectedCodes = ["03000000"],
            SearchQuery = "test"
        };

        var rootCodes = new List<CpvCodeDto>
        {
            new() { Code = "03000000", DescriptionEn = "Agricultural", DescriptionCy = "Amaethyddol", Level = 1 }
        };

        var result = originalModel.WithRootCodes(rootCodes);

        result.RootCodes.Should().BeEquivalentTo(rootCodes);
        result.SelectedCodes.Should().BeEquivalentTo(originalModel.SelectedCodes);
        result.SearchQuery.Should().Be("test");
        result.IsLoading.Should().BeFalse();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void WithError_CreatesNewInstanceWithErrorMessage()
    {
        var originalModel = new CpvSelectionViewModel
        {
            SelectedCodes = ["03000000"],
            SearchQuery = "test"
        };

        var result = originalModel.WithError("Something went wrong");

        result.ErrorMessage.Should().Be("Something went wrong");
        result.SelectedCodes.Should().BeEquivalentTo(originalModel.SelectedCodes);
        result.SearchQuery.Should().Be("test");
        result.IsLoading.Should().BeFalse();
    }

    [Fact]
    public void WithLoading_CreatesNewInstanceWithLoadingState()
    {
        var originalModel = new CpvSelectionViewModel
        {
            SelectedCodes = ["03000000"],
            SearchQuery = "test",
            ErrorMessage = "Previous error"
        };

        var result = originalModel.WithLoading(true);

        result.IsLoading.Should().BeTrue();
        result.SelectedCodes.Should().BeEquivalentTo(originalModel.SelectedCodes);
        result.SearchQuery.Should().Be("test");
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void WithLoading_False_CreatesNewInstanceWithoutLoading()
    {
        var originalModel = new CpvSelectionViewModel
        {
            SelectedCodes = ["03000000"],
            IsLoading = true
        };

        var result = originalModel.WithLoading(false);

        result.IsLoading.Should().BeFalse();
        result.SelectedCodes.Should().BeEquivalentTo(originalModel.SelectedCodes);
    }
}