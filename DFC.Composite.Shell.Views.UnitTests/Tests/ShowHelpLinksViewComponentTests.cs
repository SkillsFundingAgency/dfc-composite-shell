using DFC.Composite.Shell.ViewComponents;
using Xunit;

namespace DFC.Composite.Shell.Views.Test.Tests
{
    public class ShowHelpLinksViewComponentTests
    {
        private readonly ShowHelpLinksViewComponent viewComponent;

        public ShowHelpLinksViewComponentTests()
        {
            viewComponent = new ShowHelpLinksViewComponent();
        }

        [Fact]
        public void ReturnsPathModelDetailsWhenPathExists()
        {
            var result = viewComponent.Invoke();

            Assert.NotNull(result);
        }
    }
}
