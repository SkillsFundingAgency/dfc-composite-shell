using DFC.Composite.Shell.Models.Common;
using DFC.Composite.Shell.Services.HeaderCountService;
using Xunit;

namespace DFC.Composite.Shell.UnitTests.HeaderCounterTests
{
    public class CountTests
    {
        private readonly IHeaderCountService headerCountService;

        public CountTests()
        {
            headerCountService = new HeaderCountService();
        }

        [Theory]
        [InlineData(Constants.DfcSession, 1)]
        [InlineData("v1", int.MaxValue)]
        public void CanCount(string headerValue, int expectedHeaderCount)
        {
            Assert.Equal(expectedHeaderCount, headerCountService.Count(headerValue));
        }
    }
}
