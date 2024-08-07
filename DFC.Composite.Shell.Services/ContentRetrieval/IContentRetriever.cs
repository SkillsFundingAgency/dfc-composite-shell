﻿using DFC.Composite.Shell.Models.AppRegistrationModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.Composite.Shell.Models;
using Microsoft.AspNetCore.Http;

namespace DFC.Composite.Shell.Services.ContentRetrieval
{
    public interface IContentRetriever
    {
        Task<string> GetContent(string url, string path, RegionModel regionModel, bool followRedirects, string requestBaseUrl, IHeaderDictionary headers);

        Task<PostResponseModel> PostContent(string url, string path, RegionModel regionModel, IEnumerable<KeyValuePair<string, string>> formParameters, string requestBaseUrl);
    }
}