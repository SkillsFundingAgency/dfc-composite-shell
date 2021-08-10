using DFC.Composite.Shell.Models;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.Application
{
    public interface IApplicationService
    {
        /// <summary>
        /// Gets or sets the base url of the request.
        /// </summary>
        string RequestBaseUrl { get; set; }

        /// <summary>
        /// Gets the markup at the specified url and loads other related regions for the specified path.
        /// </summary>
        Task GetMarkupAsync(ApplicationModel application, PageViewModel pageModel, string requestPath, string queryString);

        /// <summary>
        /// Posts a request to the specified url with the specified form data and loads other related regions for the specified path.
        /// </summary>
        Task PostMarkupAsync(ApplicationModel application, IEnumerable<KeyValuePair<string, string>> formParameters, PageViewModel pageModel, string requestPath);

        /// <summary>
        /// Gets details of an application that includes regions, given a path.
        /// </summary>
        /// <param name="data">The request data.</param>
        /// <returns>ApplicationModel.</returns>
        Task<ApplicationModel> GetApplicationAsync(ActionGetRequestModel data);
    }
}