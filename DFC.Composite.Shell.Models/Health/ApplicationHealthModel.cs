using System.Collections.Generic;

namespace DFC.Composite.Shell.Models.Health
{
    public class ApplicationHealthModel
    {
        public string Path { get; set; }

        public string BearerToken { get; set; }

        public string HealthUrl { get; set; }

        public IEnumerable<HealthItemModel> Data { get; set; }
    }
}