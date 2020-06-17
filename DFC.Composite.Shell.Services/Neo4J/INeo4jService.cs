using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.Neo4J
{
    public interface INeo4JService
    {
        public Task InsertNewRequest(HttpRequest request);
    }
}
