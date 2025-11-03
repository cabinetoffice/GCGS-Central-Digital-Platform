using CO.CDP.RegisterOfCommercialTools.App.Models;
using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;
using FluentAssertions;

namespace CO.CDP.RegisterOfCommercialTools.App.Tests.Models;

public class LocationCodeSelectionTests
{
    [Fact]
    public void HasSelections_WhenCodesSelected_ReturnsTrue()
    {
        var selection = new LocationCodeSelection
        {
            SelectedCodes = ["UKC", "UKD"]
        };

        selection.HasSelections.Should().BeTrue();
    }

    [Fact]
    public void HasSelections_WhenNoCodesSelected_ReturnsFalse()
    {
        var selection = new LocationCodeSelection();

        selection.HasSelections.Should().BeFalse();
    }

    [Fact]
    public void SelectionSummary_ReturnsCorrectCount()
    {
        var selection = new LocationCodeSelection
        {
            SelectedCodes = ["UKC", "UKD", "UKE"]
        };

        selection.SelectionSummary.Should().Be("Selected (3)");
    }

    [Fact]
    public void SelectionSummary_WhenEmpty_ReturnsZeroCount()
    {
        var selection = new LocationCodeSelection();

        selection.SelectionSummary.Should().Be("Selected (0)");
    }

    [Fact]
    public void BrowseLinkText_WhenHasSelections_ReturnsEditText()
    {
        var selection = new LocationCodeSelection
        {
            SelectedCodes = ["UKC"]
        };

        selection.BrowseLinkText.Should().Be("Edit");
    }

    [Fact]
    public void BrowseLinkText_WhenNoSelections_ReturnsBrowseText()
    {
        var selection = new LocationCodeSelection();

        selection.BrowseLinkText.Should().Be("Browse locations");
    }

    [Fact]
    public void GetHiddenInputs_ReturnsCorrectNameValuePairs()
    {
        var selection = new LocationCodeSelection
        {
            SelectedCodes = ["UKC", "UKD", "UKE"]
        };

        var hiddenInputs = selection.GetHiddenInputs("location").ToList();

        hiddenInputs.Should().HaveCount(3);
        hiddenInputs.Should().Contain(("location", "UKC"));
        hiddenInputs.Should().Contain(("location", "UKD"));
        hiddenInputs.Should().Contain(("location", "UKE"));
    }

    [Fact]
    public void GetHiddenInputs_WithCustomFieldName_ReturnsCorrectNameValuePairs()
    {
        var selection = new LocationCodeSelection
        {
            SelectedCodes = ["UKC", "UKD"]
        };

        var hiddenInputs = selection.GetHiddenInputs("nuts").ToList();

        hiddenInputs.Should().HaveCount(2);
        hiddenInputs.Should().Contain(("nuts", "UKC"));
        hiddenInputs.Should().Contain(("nuts", "UKD"));
    }

    [Fact]
    public void GetHiddenInputs_WhenEmpty_ReturnsEmptyEnumerable()
    {
        var selection = new LocationCodeSelection();

        var hiddenInputs = selection.GetHiddenInputs("location");

        hiddenInputs.Should().BeEmpty();
    }

    [Fact]
    public void AddSelection_WhenCodeNotSelected_AddsCodeAndItem()
    {
        var selection = new LocationCodeSelection();

        selection.AddSelection("UKC", "North East (England)", "Gogledd Ddwyrain (Lloegr)");

        selection.SelectedCodes.Should().Contain("UKC");
        selection.SelectedItems.Should().HaveCount(1);

        var item = selection.SelectedItems.First();
        item.Code.Should().Be("UKC");
        item.DescriptionEn.Should().Be("North East (England)");
        item.DescriptionCy.Should().Be("Gogledd Ddwyrain (Lloegr)");
    }

    [Fact]
    public void AddSelection_WhenCodeAlreadySelected_DoesNotAddDuplicate()
    {
        var selection = new LocationCodeSelection
        {
            SelectedCodes = ["UKC"]
        };
        selection.SelectedItems.Add(new NutsCodeDto
        {
            Code = "UKC",
            DescriptionEn = "North East (England)",
            DescriptionCy = "Gogledd Ddwyrain (Lloegr)"
        });

        selection.AddSelection("UKC", "North East (England)", "Gogledd Ddwyrain (Lloegr)");

        selection.SelectedCodes.Should().HaveCount(1);
        selection.SelectedItems.Should().HaveCount(1);
    }

    [Fact]
    public void RemoveSelection_WhenCodeExists_RemovesCodeAndItem()
    {
        var selection = new LocationCodeSelection
        {
            SelectedCodes = ["UKC", "UKD"]
        };
        selection.SelectedItems.AddRange([
            new NutsCodeDto
            {
                Code = "UKC", DescriptionEn = "North East (England)", DescriptionCy = "Gogledd Ddwyrain (Lloegr)"
            },
            new NutsCodeDto { Code = "UKD", DescriptionEn = "North West (England)", DescriptionCy = "Gogledd Orllewin (Lloegr)" }
        ]);

        selection.RemoveSelection("UKC");

        selection.SelectedCodes.Should().NotContain("UKC");
        selection.SelectedCodes.Should().Contain("UKD");
        selection.SelectedItems.Should().HaveCount(1);
        selection.SelectedItems.First().Code.Should().Be("UKD");
    }

    [Fact]
    public void RemoveSelection_WhenCodeDoesNotExist_DoesNothing()
    {
        var selection = new LocationCodeSelection
        {
            SelectedCodes = ["UKC"]
        };
        selection.SelectedItems.Add(new NutsCodeDto
            { Code = "UKC", DescriptionEn = "North East (England)", DescriptionCy = "Gogledd Ddwyrain (Lloegr)" });

        selection.RemoveSelection("INVALID");

        selection.SelectedCodes.Should().HaveCount(1);
        selection.SelectedItems.Should().HaveCount(1);
    }

    [Fact]
    public void Clear_RemovesAllSelectionsAndItems()
    {
        var selection = new LocationCodeSelection
        {
            SelectedCodes = ["UKC", "UKD"]
        };
        selection.SelectedItems.AddRange([
            new NutsCodeDto
            {
                Code = "UKC", DescriptionEn = "North East (England)", DescriptionCy = "Gogledd Ddwyrain (Lloegr)"
            },
            new NutsCodeDto { Code = "UKD", DescriptionEn = "North West (England)", DescriptionCy = "Gogledd Orllewin (Lloegr)" }
        ]);

        selection.Clear();

        selection.SelectedCodes.Should().BeEmpty();
        selection.SelectedItems.Should().BeEmpty();
    }

    [Fact]
    public void ExpandedNodes_CanBeSetAndRetrieved()
    {
        var selection = new LocationCodeSelection
        {
            ExpandedNodes = ["UKC", "UKD"]
        };

        selection.ExpandedNodes.Should().BeEquivalentTo(["UKC", "UKD"]);
    }

    [Fact]
    public void ExpandedNodes_DefaultsToEmptyList()
    {
        var selection = new LocationCodeSelection();

        selection.ExpandedNodes.Should().BeEmpty();
    }

    [Fact]
    public void Properties_CanBeSetIndependently()
    {
        var selection = new LocationCodeSelection
        {
            SelectedCodes = ["UKC"],
            ExpandedNodes = ["UKC", "UKD"]
        };

        selection.SelectedCodes.Should().HaveCount(1);
        selection.ExpandedNodes.Should().HaveCount(2);
        selection.HasSelections.Should().BeTrue();
    }

}