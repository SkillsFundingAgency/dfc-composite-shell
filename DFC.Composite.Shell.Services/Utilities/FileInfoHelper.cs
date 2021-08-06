using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.Utilities
{
    [ExcludeFromCodeCoverage]
    public class FileInfoHelper : IFileInfoHelper
    {
        public bool FileExists(string fileName)
        {
            return File.Exists(fileName);
        }

        public Stream GetStream(string fileName)
        {
            return new FileStream(fileName, FileMode.Open, FileAccess.Read);
        }

        public async Task<string> ReadAllTextAsync(string file)
        {
            return await File.ReadAllTextAsync(file);
        }
    }
}