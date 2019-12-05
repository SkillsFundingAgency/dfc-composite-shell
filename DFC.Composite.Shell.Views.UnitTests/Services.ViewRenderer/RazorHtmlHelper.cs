using RazorEngine.Text;

namespace DFC.Composite.Shell.Views.Test.Services.ViewRenderer
{
    public class RazorHtmlHelper
    {
        public IEncodedString Raw(string rawString)
        {
            return new RawString(rawString);
        }
    }
}
