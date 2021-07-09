using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Models.Enums;
using Xunit;

namespace DFC.Composite.Shell.UnitTests.Models
{
    public class MarkupMessagesTests
    {
        [Fact]
        public void MarkupMessagesReturnsAppOfflineMessage()
        {
            // arrange
            var model = new MarkupMessages();

            // act
            var result = model.AppOfflineHtml;

            // assert
            Assert.Equal(model.AppOfflineHtml, result);
        }

        [Theory]
        [InlineData(PageRegion.Head, null)]
        [InlineData(PageRegion.Breadcrumb, null)]
        [InlineData(PageRegion.BodyTop, "<div class=\"govuk-width-container\"><h3>Service Unavailable</h3></div>")]
        [InlineData(PageRegion.Body, "<div class=\"govuk-width-container\"><h3>Service Unavailable</h3></div>")]
        [InlineData(PageRegion.SidebarRight, "<div class=\"govuk-width-container\"><h3>Service Unavailable</h3></div>")]
        [InlineData(PageRegion.SidebarLeft, "<div class=\"govuk-width-container\"><h3>Service Unavailable</h3></div>")]
        [InlineData(PageRegion.BodyFooter, "<div class=\"govuk-width-container\"><h3>Service Unavailable</h3></div>")]
        [InlineData(PageRegion.HeroBanner, "<div class=\"govuk-width-container\"><h3>Service Unavailable</h3></div>")]
        public void MarkupMessagesReturnsRegionOfflineMessage(PageRegion pageRegion, string expected)
        {
            // arrange
            var model = new MarkupMessages();

            // act
            var result = model.GetRegionOfflineHtml(pageRegion);

            // assert
            Assert.Equal(expected, result);
        }
    }
}
