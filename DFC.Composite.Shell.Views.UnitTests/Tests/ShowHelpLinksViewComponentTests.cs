using DFC.Composite.Shell.Models.AppRegistrationModels;
using DFC.Composite.Shell.Services.AppRegistry;
using DFC.Composite.Shell.ViewComponents;
using DFC.Composite.Shell.Views.Test.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
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
            var result =  viewComponent.Invoke();

            Assert.NotNull(result);
        }
    }
}
