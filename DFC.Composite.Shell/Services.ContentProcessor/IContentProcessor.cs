﻿namespace DFC.Composite.Shell.Services.ContentProcessor
{
    public interface IContentProcessor
    {
        string Process(string content, string requestBaseUrl, string applicationRootUrl);
    }
}
