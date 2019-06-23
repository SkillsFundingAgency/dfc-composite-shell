using DFC.Composite.Shell.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.Application
{
    public interface IApplicationService
    {
        /// <summary>
        /// Gets the markup at the specified url and loads other related regions for the specified path
        /// </summary>
        /// <param name="application">The application model</param>
        /// <param name="article">The relative url</param>
        /// <param name="pageModel">A page model object, that has its path set</param>
        Task GetMarkupAsync(ApplicationModel application, string article, PageViewModel pageModel);

        /// <summary>
        /// Posts a request to the specified url with the specified form data and loads other related regions for the specified path
        /// </summary>
        /// <param name="application">The application model</param>
        /// <param name="path">The path for the application</param>
        /// <param name="article">The relative url under the path</param>
        /// <param name="formParameters"></param>
        /// <param name="pageModel"></param>
        /// <returns></returns>
        Task PostMarkupAsync(ApplicationModel application, string path, string article, IEnumerable<KeyValuePair<string, string>> formParameters, PageViewModel pageModel);

        /// <summary>
        /// Gets details of an application that includes regions, given a path
        /// </summary>
        /// <param name="path">The path or application name</param>
        /// <returns></returns>
        Task<ApplicationModel> GetApplicationAsync(string path);
    }
}
