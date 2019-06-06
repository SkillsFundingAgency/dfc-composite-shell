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
        /// <param name="path">The path or application name</param>
        /// <param name="contentUrl">The relative url</param>
        /// <param name="pageModel">A page model object, that has its path set</param>
        Task GetMarkupAsync(string path, string contentUrl, PageViewModel pageModel);

        /// <summary>
        /// Posts a request to the specified url with the specified form data and loads other related regions for the specified path
        /// </summary>
        /// <param name="path">The path or application name</param>
        /// <param name="contentUrl">The relative url</param>
        /// <param name="formParameters"></param>
        /// <param name="pageModel"></param>
        /// <returns></returns>
        Task PostMarkupAsync(string path, string contentUrl, IEnumerable<KeyValuePair<string, string>> formParameters, PageViewModel pageModel);

        /// <summary>
        /// Gets details of an application that includes regions, given a path
        /// </summary>
        /// <param name="path">The path or application name</param>
        /// <returns></returns>
        Task<ApplicationModel> GetApplicationAsync(string path);
    }
}
