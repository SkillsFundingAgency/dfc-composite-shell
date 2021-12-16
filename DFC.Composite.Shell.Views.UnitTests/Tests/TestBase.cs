using System.IO;

namespace DFC.Composite.Shell.Views.Test.Tests
{
    public class TestBase
    {
        protected string ViewRootPath => $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}DFC.Composite.Shell{Path.DirectorySeparatorChar}";
    }
}
