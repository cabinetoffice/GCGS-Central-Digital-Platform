using CO.CDP.RegisterOfCommercialTools.App.Models;
using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;
using FluentAssertions;

namespace CO.CDP.RegisterOfCommercialTools.App.Tests.Models;

public class CpvTreeNodeViewModelTests
{
    [Fact]
    public void IsSelected_WhenCodeIsInSelectedCodes_ReturnsTrue()
    {
        var cpvItem = new CpvCodeDto { Code = "03000000", DescriptionEn = "Agricultural products", DescriptionCy = "Cynhyrchion amaethyddol", Level = 1 };
        var viewModel = new CpvTreeNodeViewModel
        {
            Item = cpvItem,
            SelectedCodes = ["03000000", "03100000"]
        };

        viewModel.IsSelected.Should().BeTrue();
    }

    [Fact]
    public void IsSelected_WhenCodeIsNotInSelectedCodes_ReturnsFalse()
    {
        var cpvItem = new CpvCodeDto { Code = "03000000", DescriptionEn = "Agricultural products", DescriptionCy = "Cynhyrchion amaethyddol", Level = 1 };
        var viewModel = new CpvTreeNodeViewModel
        {
            Item = cpvItem,
            SelectedCodes = ["03100000", "09000000"]
        };

        viewModel.IsSelected.Should().BeFalse();
    }

    [Fact]
    public void IsSelected_WhenSelectedCodesEmpty_ReturnsFalse()
    {
        var cpvItem = new CpvCodeDto { Code = "03000000", DescriptionEn = "Agricultural products", DescriptionCy = "Cynhyrchion amaethyddol", Level = 1 };
        var viewModel = new CpvTreeNodeViewModel
        {
            Item = cpvItem,
            SelectedCodes = []
        };

        viewModel.IsSelected.Should().BeFalse();
    }

    [Fact]
    public void HasChildren_WhenItemHasChildren_ReturnsTrue()
    {
        var cpvItem = new CpvCodeDto
        {
            Code = "03000000",
            DescriptionEn = "Agricultural products",
            DescriptionCy = "Cynhyrchion amaethyddol",
            Level = 1,
            HasChildren = true
        };
        var viewModel = new CpvTreeNodeViewModel
        {
            Item = cpvItem,
            SelectedCodes = []
        };

        viewModel.HasChildren.Should().BeTrue();
    }

    [Fact]
    public void HasChildren_WhenItemHasNoChildren_ReturnsFalse()
    {
        var cpvItem = new CpvCodeDto
        {
            Code = "03110000",
            DescriptionEn = "Seeds",
            DescriptionCy = "Hadau",
            Level = 3,
            HasChildren = false
        };
        var viewModel = new CpvTreeNodeViewModel
        {
            Item = cpvItem,
            SelectedCodes = []
        };

        viewModel.HasChildren.Should().BeFalse();
    }

    [Fact]
    public void CheckboxId_GeneratesUniqueIdBasedOnCode()
    {
        var cpvItem = new CpvCodeDto { Code = "03000000", DescriptionEn = "Agricultural products", DescriptionCy = "Cynhyrchion amaethyddol", Level = 1 };
        var viewModel = new CpvTreeNodeViewModel
        {
            Item = cpvItem,
            SelectedCodes = []
        };

        viewModel.CheckboxId.Should().Be("cpv-checkbox-03000000");
    }

    [Fact]
    public void CheckboxId_WithNonNumericCharacters_StillGeneratesValidId()
    {
        var cpvItem = new CpvCodeDto { Code = "03-000-000", DescriptionEn = "Test product", DescriptionCy = "Cynnyrch prawf", Level = 1 };
        var viewModel = new CpvTreeNodeViewModel
        {
            Item = cpvItem,
            SelectedCodes = []
        };

        viewModel.CheckboxId.Should().Be("cpv-checkbox-03-000-000");
    }

    [Fact]
    public void Level_ReturnsItemLevel()
    {
        var cpvItem = new CpvCodeDto { Code = "03100000", DescriptionEn = "Agricultural products", DescriptionCy = "Cynhyrchion amaethyddol", Level = 2 };
        var viewModel = new CpvTreeNodeViewModel
        {
            Item = cpvItem,
            Level = 2,
            SelectedCodes = []
        };

        viewModel.Level.Should().Be(2);
    }

    [Fact]
    public void Constructor_SetsPropertiesCorrectly()
    {
        var cpvItem = new CpvCodeDto
        {
            Code = "03000000",
            DescriptionEn = "Agricultural products",
            DescriptionCy = "Cynhyrchion amaethyddol",
            Level = 1,
            HasChildren = true
        };
        var selectedCodes = new List<string> { "03000000", "03100000" };

        var viewModel = new CpvTreeNodeViewModel
        {
            Item = cpvItem,
            Level = 1,
            SelectedCodes = selectedCodes
        };

        viewModel.Item.Should().Be(cpvItem);
        viewModel.Level.Should().Be(1);
        viewModel.SelectedCodes.Should().BeEquivalentTo(selectedCodes);
        viewModel.IsSelected.Should().BeTrue();
        viewModel.HasChildren.Should().BeTrue();
        viewModel.CheckboxId.Should().Be("cpv-checkbox-03000000");
    }
}