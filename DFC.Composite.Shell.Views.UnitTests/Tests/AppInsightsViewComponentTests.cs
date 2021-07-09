using DFC.Composite.Shell.Models.Common;
using DFC.Composite.Shell.ViewComponents;
using DFC.Composite.Shell.Views.Test.Extensions;
using FakeItEasy;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.Shell.Views.Test.Tests
{
    public class AppInsightsViewComponentTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("someKey")]
        public async Task ReturnsAppInsightsKeyWhenItExists(string expectedInsightsKey)
        {
            // Arrange
            var configuration = A.Fake<IConfiguration>();
            var configSection = A.Fake<IConfigurationSection>();
            configSection.Value = expectedInsightsKey;

            A.CallTo(() => configuration.GetSection(Constants.ApplicationInsightsInstrumentationKey)).Returns(configSection);
            var viewComponent = new AppInsightsViewComponent(configuration);

            // Act
            var result = await viewComponent.InvokeAsync();

            // Assert
            var viewComponentModel = result.ViewDataModelAs<AppInsightsViewModel>();
            Assert.Equal(expectedInsightsKey, viewComponentModel.InstrumentationKey);
        }
    }
}
