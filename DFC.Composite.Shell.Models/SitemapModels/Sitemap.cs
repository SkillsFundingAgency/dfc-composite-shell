using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                if (value != null)
                {
                    map.Clear();
                    AddRange(value);
                }
            }
        }

        public void AddRange(IEnumerable<SitemapLocation> locs)
        {
            if (locs != null)
            {
                foreach (var i in locs)
                {
                    var existingMap = map.ToArray().FirstOrDefault(f => (f as SitemapLocation).Url.Equals(i.Url, StringComparison.OrdinalIgnoreCase));
                    if (existingMap == null)
                    {
                        _ = map.Add(i);
                    }
                }
            }
        }

        public string WriteSitemapToString()
        {
            using StringWriter sw = new StringWriter();
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("xhtml", "http://www.w3.org/1999/xhtml");

            XmlSerializer xs = new XmlSerializer(typeof(Sitemap));
            xs.Serialize(sw, this, ns);
            return sw.GetStringBuilder().ToString();
        }
    }
}