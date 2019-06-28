using System.Collections.Generic;

namespace DFC.Composite.Shell.Models
{
    public class ApplicationModel
    {
        public PathModel Path { get; set; }
        public IEnumerable<RegionModel> Regions { get; set; }
        public string RootUrl{ get; set; }
    }
}
