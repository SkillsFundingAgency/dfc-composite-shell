using DFC.Composite.Shell.Models.Common;
using DFC.Composite.Shell.Services.HeaderRenamer;
using Xunit;

namespace DFC.Composite.Shell.UnitTests.HeaderRenamerTests
{
    public class RenameTests
    {
        private readonly IHeaderRenamerService headerRenamerService;

        public RenameTests()
        {
            headerRenamerService = new HeaderRenamerService();
        }

        [Theory]
        [InlineData(Constants.DfcSession, false)]
        [InlineData("v1", true)]
        public void CanRename(string headerValue, bool renameHeader)
        {
            Assert.Equal(renameHeader, headerRenamerService.Rename(headerValue));
        }
    }
}
