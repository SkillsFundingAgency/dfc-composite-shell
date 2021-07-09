using DFC.Composite.Shell.Services.UrlRewriter;
using System;

namespace DFC.Composite.Shell.Services.ContentProcessor
{
    public class ContentProcessorService : IContentProcessorService
    {
        private readonly IUrlRewriterService urlRewriterService;

        public ContentProcessorService(IUrlRewriterService urlRewriterService)
        {
            this.urlRewriterService = urlRewriterService;
        }

        public string Process(string content, Uri requestBaseUrl, Uri applicationRootUrl)
        {
            return !string.IsNullOrWhiteSpace(content) ?
                urlRewriterService.RewriteAttributes(content, requestBaseUrl, applicationRootUrl) : string.Empty;
        }
    }
}