using System.Xml.Serialization;

namespace DFC.Composite.Shell.Models.SitemapModels
{
    [XmlType(Namespace = "http://www.w3.org/1999/xhtml")]
    public class SitemapImage
    {
        [XmlElement("loc")]
        public string Location { get; set; }

        [XmlElement("caption")]
        public string Caption { get; set; }

        [XmlElement("title")]
        public string Title { get; set; }

        [XmlElement("geo_location")]
        public string GeoLocation { get; set; }

        public bool ShouldSerializeCaption()
        {
            return !string.IsNullOrWhiteSpace(Caption);
        }

        public bool ShouldSerializeTitle()
        {
            return !string.IsNullOrWhiteSpace(Title);
        }

        public bool ShouldSerializeGeoLoacation()
        {
            return !string.IsNullOrWhiteSpace(GeoLocation);
        }
    }
}