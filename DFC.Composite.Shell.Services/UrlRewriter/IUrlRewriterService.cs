using System;

namespace DFC.Composite.Shell.Services.UrlRewriter
{
    public interface IUrlRewriterService
    {
        string RewriteAttributes(string content, Uri requestBaseUrl, Uri applicationRootUrl);
    }
}