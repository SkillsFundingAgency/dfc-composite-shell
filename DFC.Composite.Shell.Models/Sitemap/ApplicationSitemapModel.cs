using System.Collections.Generic;

namespace DFC.Composite.Shell.Models.Sitemap
{
    public class ApplicationSitemapModel
    {
        public string Path { get; set; }

        public string BearerToken { get; set; }

        public string SitemapUrl { get; set; }

        public IEnumerable<SitemapLocation> Data { get; set; }
    }
}