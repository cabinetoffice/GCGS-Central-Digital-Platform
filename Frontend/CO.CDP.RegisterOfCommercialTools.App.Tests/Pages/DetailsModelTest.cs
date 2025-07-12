using CO.CDP.RegisterOfCommercialTools.App.Models;
using CO.CDP.RegisterOfCommercialTools.App.Pages;
using CO.CDP.RegisterOfCommercialTools.App.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.RegisterOfCommercialTools.App.Tests.Pages
{
    public class DetailsModelTest
    {
        [Fact]
        public async Task OnGetAsync_ReturnsPage_WhenResultFound()
        {
            var id = Guid.NewGuid();
            var expectedResult = new SearchResult(
                id,
                "Test Title",
                "Test Caption",
                "Test Tool",
                SearchResultStatus.Active,
                "1%",
                "Yes",
                "2025-12-31",
                "2026-01-01 to 2027-01-01",
                "Direct award"
            );
            var searchServiceMock = new Mock<ISearchService>();
            searchServiceMock.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(expectedResult);
            var model = new DetailsModel(searchServiceMock.Object);

            var result = await model.OnGetAsync(id);

            Assert.IsType<PageResult>(result);
            Assert.Equal(expectedResult, model.Result);
        }

        [Fact]
        public async Task OnGetAsync_RedirectsToNotFound_WhenResultNotFound()
        {
            var id = Guid.NewGuid();
            var searchServiceMock = new Mock<ISearchService>();
            searchServiceMock.Setup(s => s.GetByIdAsync(id)).ReturnsAsync((SearchResult?)null);
            var model = new DetailsModel(searchServiceMock.Object);

            var result = await model.OnGetAsync(id);

            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/NotFound", redirectResult.PageName);
            Assert.Null(model.Result);
        }
    }
}
