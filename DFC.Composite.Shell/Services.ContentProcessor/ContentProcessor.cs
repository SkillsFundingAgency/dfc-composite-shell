using DFC.Composite.Shell.Services.UrlRewriter;

namespace DFC.Composite.Shell.Services.ContentProcessor
{
    public class ContentProcessor : IContentProcessor
    {
        private readonly IUrlRewriter _urlRewriter;

        public ContentProcessor(IUrlRewriter urlRewriter)
        {
            _urlRewriter = urlRewriter;
        }

        public string Process(string content, string requestBaseUrl, string applicationRootUrl)
        {
            var result = content;
            if (!string.IsNullOrWhiteSpace(content))
            {
                result = _urlRewriter.Rewrite(content, requestBaseUrl, applicationRootUrl);
            }
            return result;
        }
    }
}
