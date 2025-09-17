using CO.CDP.RegisterOfCommercialTools.App.Models;
using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.RegisterOfCommercialTools.App.Tests.Models;

public class CpvCodeSelectionTests
{
    [Fact]
    public void HasSelections_WhenCodesSelected_ReturnsTrue()
    {
        var selection = new CpvCodeSelection
        {
            SelectedCodes = ["03000000", "03100000"]
        };

        selection.HasSelections.Should().BeTrue();
    }

    [Fact]
    public void HasSelections_WhenNoCodesSelected_ReturnsFalse()
    {
        var selection = new CpvCodeSelection();

        selection.HasSelections.Should().BeFalse();
    }

    [Fact]
    public void SelectionSummary_ReturnsCorrectCount()
    {
        var selection = new CpvCodeSelection
        {
            SelectedCodes = ["03000000", "03100000", "03200000"]
        };

        selection.SelectionSummary.Should().Be("Selected (3)");
    }

    [Fact]
    public void SelectionSummary_WhenEmpty_ReturnsZeroCount()
    {
        var selection = new CpvCodeSelection();

        selection.SelectionSummary.Should().Be("Selected (0)");
    }

    [Fact]
    public void BrowseLinkText_WhenHasSelections_ReturnsEditText()
    {
        var selection = new CpvCodeSelection
        {
            SelectedCodes = ["03000000"]
        };

        selection.BrowseLinkText.Should().Be("Edit CPV code selection");
    }

    [Fact]
    public void BrowseLinkText_WhenNoSelections_ReturnsBrowseText()
    {
        var selection = new CpvCodeSelection();

        selection.BrowseLinkText.Should().Be("Browse CPV codes");
    }

    [Fact]
    public void GetHiddenInputs_ReturnsCorrectNameValuePairs()
    {
        var selection = new CpvCodeSelection
        {
            SelectedCodes = ["03000000", "03100000", "09000000"]
        };

        var hiddenInputs = selection.GetHiddenInputs().ToList();

        hiddenInputs.Should().HaveCount(3);
        hiddenInputs.Should().Contain(("cpv", "03000000"));
        hiddenInputs.Should().Contain(("cpv", "03100000"));
        hiddenInputs.Should().Contain(("cpv", "09000000"));
    }

    [Fact]
    public void GetHiddenInputs_WhenEmpty_ReturnsEmptyEnumerable()
    {
        var selection = new CpvCodeSelection();

        var hiddenInputs = selection.GetHiddenInputs();

        hiddenInputs.Should().BeEmpty();
    }

    [Fact]
    public void AddSelection_WhenCodeNotSelected_AddsCodeAndItem()
    {
        var selection = new CpvCodeSelection();

        selection.AddSelection("03000000", "Agricultural products", "Cynhyrchion amaethyddol");

        selection.SelectedCodes.Should().Contain("03000000");
        selection.SelectedItems.Should().HaveCount(1);

        var item = selection.SelectedItems.First();
        item.Code.Should().Be("03000000");
        item.DescriptionEn.Should().Be("Agricultural products");
        item.DescriptionCy.Should().Be("Cynhyrchion amaethyddol");
    }

    [Fact]
    public void AddSelection_WhenCodeAlreadySelected_DoesNotAddDuplicate()
    {
        var selection = new CpvCodeSelection
        {
            SelectedCodes = ["03000000"]
        };
        selection.SelectedItems.Add(new CpvCodeDto
        {
            Code = "03000000",
            DescriptionEn = "Agricultural products",
            DescriptionCy = "Cynhyrchion amaethyddol"
        });

        selection.AddSelection("03000000", "Agricultural products", "Cynhyrchion amaethyddol");

        selection.SelectedCodes.Should().HaveCount(1);
        selection.SelectedItems.Should().HaveCount(1);
    }

    [Fact]
    public void RemoveSelection_WhenCodeExists_RemovesCodeAndItem()
    {
        var selection = new CpvCodeSelection
        {
            SelectedCodes = ["03000000", "03100000"]
        };
        selection.SelectedItems.AddRange([
            new CpvCodeDto
            {
                Code = "03000000", DescriptionEn = "Agricultural products", DescriptionCy = "Cynhyrchion amaethyddol"
            },
            new CpvCodeDto { Code = "03100000", DescriptionEn = "Live animals", DescriptionCy = "Anifeiliaid byw" }
        ]);

        selection.RemoveSelection("03000000");

        selection.SelectedCodes.Should().NotContain("03000000");
        selection.SelectedCodes.Should().Contain("03100000");
        selection.SelectedItems.Should().HaveCount(1);
        selection.SelectedItems.First().Code.Should().Be("03100000");
    }

    [Fact]
    public void RemoveSelection_WhenCodeDoesNotExist_DoesNothing()
    {
        var selection = new CpvCodeSelection
        {
            SelectedCodes = ["03000000"]
        };
        selection.SelectedItems.Add(new CpvCodeDto
            { Code = "03000000", DescriptionEn = "Agricultural products", DescriptionCy = "Cynhyrchion amaethyddol" });

        selection.RemoveSelection("99999999");

        selection.SelectedCodes.Should().HaveCount(1);
        selection.SelectedItems.Should().HaveCount(1);
    }

    [Fact]
    public void Clear_RemovesAllSelectionsAndItems()
    {
        var selection = new CpvCodeSelection
        {
            SelectedCodes = ["03000000", "03100000"]
        };
        selection.SelectedItems.AddRange([
            new CpvCodeDto
            {
                Code = "03000000", DescriptionEn = "Agricultural products", DescriptionCy = "Cynhyrchion amaethyddol"
            },
            new CpvCodeDto { Code = "03100000", DescriptionEn = "Live animals", DescriptionCy = "Anifeiliaid byw" }
        ]);

        selection.Clear();

        selection.SelectedCodes.Should().BeEmpty();
        selection.SelectedItems.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithNoSelections_ReturnsValid()
    {
        var selection = new CpvCodeSelection();
        var validationContext = new ValidationContext(selection);

        var result = selection.Validate(validationContext);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithSelections_ReturnsValid()
    {
        var selection = new CpvCodeSelection
        {
            SelectedCodes = ["03000000", "03100000", "03200000"]
        };
        var validationContext = new ValidationContext(selection);

        var result = selection.Validate(validationContext);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithManySelections_ReturnsValid()
    {
        var selection = new CpvCodeSelection();

        for (int i = 0; i < 50; i++)
        {
            selection.SelectedCodes.Add($"030{i:D5}00");
        }

        var validationContext = new ValidationContext(selection);
        var result = selection.Validate(validationContext);

        result.Should().BeEmpty();
    }

    [Fact]
    public void ExpandedNodes_CanBeSetAndRetrieved()
    {
        var selection = new CpvCodeSelection
        {
            ExpandedNodes = ["03000000", "03100000"]
        };

        selection.ExpandedNodes.Should().BeEquivalentTo(["03000000", "03100000"]);
    }

    [Fact]
    public void ExpandedNodes_DefaultsToEmptyList()
    {
        var selection = new CpvCodeSelection();

        selection.ExpandedNodes.Should().BeEmpty();
    }

    [Fact]
    public void Properties_CanBeSetIndependently()
    {
        var selection = new CpvCodeSelection
        {
            SelectedCodes = ["03000000"],
            ExpandedNodes = ["03000000", "03100000"]
        };

        selection.SelectedCodes.Should().HaveCount(1);
        selection.ExpandedNodes.Should().HaveCount(2);
        selection.HasSelections.Should().BeTrue();
    }
}