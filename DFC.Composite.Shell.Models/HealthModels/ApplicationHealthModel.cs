using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Models.HealthModels
{
    public class ApplicationHealthModel
    {
        public string Path { get; set; }

        public string BearerToken { get; set; }

        public string HealthUrl { get; set; }

        public Task<IEnumerable<HealthItemModel>> RetrievalTask { get; set; }
    }
}