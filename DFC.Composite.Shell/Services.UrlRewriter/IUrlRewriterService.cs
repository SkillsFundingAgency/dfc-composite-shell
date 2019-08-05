namespace DFC.Composite.Shell.Services.UrlRewriter
{
    public interface IUrlRewriterService
    {
        string Rewrite(string content, string requestBaseUrl, string applicationRootUrl);
    }
}