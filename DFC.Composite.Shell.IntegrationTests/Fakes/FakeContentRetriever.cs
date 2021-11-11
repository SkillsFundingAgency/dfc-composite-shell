using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Models.AppRegistrationModels;
using DFC.Composite.Shell.Services.ContentRetrieval;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.IntegrationTests.Fakes
{
    public class FakeContentRetriever : IContentRetriever
    {
        public const string FileContentType = "application/pdf";
        public const string FileName = "FileName.pdf";

        public Task<string> GetContent(
            string url,
            string path,
            RegionModel regionModel,
            bool followRedirects,
            string requestBaseUrl,
            IHeaderDictionary headers)
        {
            return Task.FromResult(Concat(
                "GET",
                url,
                path,
                regionModel?.PageRegion.ToString()));
        }

        public Task<PostResponseModel> PostContent(
            string url,
            string path,
            RegionModel regionModel,
            IEnumerable<KeyValuePair<string, string>> formParameters,
            string requestBaseUrl)
        {
            return formParameters.Any(f => f.Key.Equals("download", StringComparison.InvariantCultureIgnoreCase))
                ? Task.FromResult(new PostResponseModel
                {
                    FileDownloadModel = new FileDownloadModel
                    {
                        FileContentType = FileContentType,
                        FileName = FileName,
                        FileBytes = Array.Empty<byte>(),
                    },
                })
                : Task.FromResult(new PostResponseModel
                {
                    Html = Concat(
                        "POST",
                        url,
                        path,
                        regionModel?.PageRegion.ToString(),
                        string.Join(", ", formParameters.Select(kvp => string.Concat(kvp.Key, "=", kvp.Value)))),
                });
        }

        private string Concat(params string[] values)
        {
            return string.Join(", ", values);
        }
    }
}
