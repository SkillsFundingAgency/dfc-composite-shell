using System;

namespace DFC.Composite.Shell.Services.ContentProcessor
{
    public interface IContentProcessorService
    {
        string Process(string content, Uri requestBaseUrl, Uri applicationRootUrl);
    }
}