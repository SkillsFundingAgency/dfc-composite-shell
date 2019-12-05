using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace DFC.Composite.Shell.Models.SitemapModels
{
    [XmlRoot("urlset", Namespace = "http://www.sitemaps.org/schemas/sitemap/0.9")]
    public class Sitemap
    {
        private readonly ArrayList map;

        public Sitemap()
        {
            map = new ArrayList();
        }

        [XmlIgnore]
        public IEnumerable<SitemapLocation> Mappings => map as IEnumerable<SitemapLocation>;

        [XmlElement("url")]
        public SitemapLocation[] Locations
        {
            get
            {
                SitemapLocation[] items = new SitemapLocation[map.Count];
                map.CopyTo(items);
                return items;
            }

            set
            {
                if (value == null)
                {
                    return;
                }

                var items = value;
                map.Clear();
                foreach (var sitemapLocation in items)
                {
                    map.Add(sitemapLocation);
                }
            }
        }

        public static string GetSitemapXml()
        {
            return string.Empty;
        }

        public int Add(SitemapLocation item)
        {
            return map.Add(item);
        }

        public void AddRange(IEnumerable<SitemapLocation> locs)
        {
            if (locs == null)
            {
                return;
            }

            foreach (var i in locs)
            {
                map.Add(i);
            }
        }

        public void WriteSitemapToFile(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                var ns = new XmlSerializerNamespaces();
                ns.Add("image", "http://www.google.com/schemas/sitemap-image/1.1");

                var xs = new XmlSerializer(typeof(Sitemap));
                xs.Serialize(fs, this, ns);
            }
        }

        public string WriteSitemapToString()
        {
            using (StringWriter sw = new StringWriter())
            {
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("image", "http://www.google.com/schemas/sitemap-image/1.1");

                XmlSerializer xs = new XmlSerializer(typeof(Sitemap));
                xs.Serialize(sw, this, ns);
                return sw.GetStringBuilder().ToString();
            }
        }
    }
}