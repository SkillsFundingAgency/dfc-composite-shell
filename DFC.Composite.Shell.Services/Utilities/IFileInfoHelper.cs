using System.IO;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.Utilities
{
    public interface IFileInfoHelper
    {
        bool FileExists(string fileName);

        Stream GetStream(string fileName);

        Task<string> ReadAllTextAsync(string file);
    }
}