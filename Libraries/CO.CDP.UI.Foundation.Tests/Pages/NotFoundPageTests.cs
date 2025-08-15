using Xunit;
using CO.CDP.UI.Foundation.Pages;

namespace CO.CDP.UI.Foundation.Tests.Pages
{
    public class NotFoundPageTests
    {
        [Fact]
        public void Render_DefaultTitle_ReturnsExpectedHtml()
        {
            var page = new NotFoundPage();
            var expectedTitle = "Page not found";

            var result = page.Render();

            Assert.Contains($"<h1 class=\"govuk-heading-l\">{expectedTitle}</h1>", result);
            Assert.Contains("govuk-grid-row", result);
            Assert.Contains("govuk-grid-column-two-thirds", result);
            Assert.Contains("govuk-body", result);
        }

        [Fact]
        public void Render_CustomTitle_ReturnsHtmlWithCustomTitle()
        {
            var page = new NotFoundPage();
            var customTitle = "Custom Not Found";

            var result = page.Render(customTitle);

            Assert.Contains($"<h1 class=\"govuk-heading-l\">{customTitle}</h1>", result);
        }

        [Fact]
        public void Render_Always_ReturnsSemanticHtml()
        {
            var page = new NotFoundPage();

            var result = page.Render();

            Assert.Contains("<div class=\"govuk-grid-row\">", result);
            Assert.Contains("<div class=\"govuk-grid-column-two-thirds\">", result);
            Assert.Contains("<h1 class=\"govuk-heading-l\">", result);
            Assert.Contains("<p class=\"govuk-body\">", result);
        }
    }
}