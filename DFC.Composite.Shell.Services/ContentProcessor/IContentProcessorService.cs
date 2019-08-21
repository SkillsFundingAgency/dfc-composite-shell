namespace DFC.Composite.Shell.Services.ContentProcessor
{
    public interface IContentProcessorService
    {
        string Process(string content, string requestBaseUrl, string applicationRootUrl);
    }
}