using System;
using System.Xml.Serialization;

namespace DFC.Composite.Shell.Models.SitemapModels
{
    public class SitemapLocation
    {
        [XmlElement("loc")]
        public string Url { get; set; }

        [XmlElement("changefreq")]
        public ChangeFrequency? ChangeFrequency { get; set; } = SitemapModels.ChangeFrequency.Monthly;

        [XmlElement("lastmod")]
        public DateTime? LastModified { get; set; }

        [XmlElement("priority")]
        public double? Priority { get; set; } = 0.5;

        public bool ShouldSerializeChangeFrequency()
        {
            return ChangeFrequency.HasValue;
        }

        public bool ShouldSerializeLastModified()
        {
            return LastModified.HasValue;
        }

        public bool ShouldSerializePriority()
        {
            return Priority.HasValue;
        }
    }
}