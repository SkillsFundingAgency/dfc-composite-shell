using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace DFC.Composite.Shell.Models.Sitemap
{
    [XmlRoot("urlset", Namespace = "http://www.sitemaps.org/schemas/sitemap/0.9")]
    public class Sitemap
    {
        private List<SitemapLocation> locationMap = new List<SitemapLocation>();

        [XmlElement("url")]
        public SitemapLocation[] Locations
        {
            get
            {
                return locationMap.ToArray();
            }
            set
            {
                locationMap = value?.ToList();
            }
        }

        public void AddRange(IEnumerable<SitemapLocation> sitemapLocations)
        {
            locationMap?.AddRange(sitemapLocations);
        }

        public string WriteSitemapToString()
        {
            using var stringWriter = new StringWriter();
            GetXmlSerializer().Serialize(stringWriter, this, GetNamespace());

            return stringWriter.GetStringBuilder().ToString();
        }

        private static XmlSerializerNamespaces GetNamespace()
        {
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add("image", "http://www.google.com/schemas/sitemap-image/1.1");

            return namespaces;
        }

        private static XmlSerializer GetXmlSerializer()
        {
            return new XmlSerializer(typeof(Sitemap));
        }
    }
}