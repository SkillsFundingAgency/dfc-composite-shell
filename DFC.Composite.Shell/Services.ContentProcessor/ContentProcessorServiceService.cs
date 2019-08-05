using DFC.Composite.Shell.Services.UrlRewriter;

namespace DFC.Composite.Shell.Services.ContentProcessor
{
    public class ContentProcessorServiceService : IContentProcessorService
    {
        private readonly IUrlRewriterService urlRewriterService;

        public ContentProcessorServiceService(IUrlRewriterService urlRewriterService)
        {
            this.urlRewriterService = urlRewriterService;
        }

        public string Process(string content, string requestBaseUrl, string applicationRootUrl)
        {
            return !string.IsNullOrWhiteSpace(content) ? urlRewriterService.Rewrite(content, requestBaseUrl, applicationRootUrl) : string.Empty;
        }
    }
}