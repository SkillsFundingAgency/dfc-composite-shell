namespace DFC.Composite.Shell.Services.UrlRewriter
{
    public interface IUrlRewriter
    {
        string Rewrite(string content, string requestBaseUrl, string applicationRootUrl);
    }
}
