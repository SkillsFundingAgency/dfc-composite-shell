using DFC.Composite.Shell.Services.UrlRewriter;

namespace DFC.Composite.Shell.Services.ContentProcessor
{
    public class ContentProcessorService : IContentProcessorService
    {
        private readonly IUrlRewriterService urlRewriterService;

        public ContentProcessorService(IUrlRewriterService urlRewriterService)
        {
            this.urlRewriterService = urlRewriterService;
        }

        public string Process(string content, string requestBaseUrl, string applicationRootUrl)
        {
            return !string.IsNullOrWhiteSpace(content) ? urlRewriterService.Rewrite(content, requestBaseUrl, applicationRootUrl) : string.Empty;
        }
    }
}