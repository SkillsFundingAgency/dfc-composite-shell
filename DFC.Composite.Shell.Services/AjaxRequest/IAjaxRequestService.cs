using DFC.Composite.Shell.Models.AjaxApiModels;
using DFC.Composite.Shell.Models.AppRegistrationModels;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.AjaxRequest
{
    public interface IAjaxRequestService
    {
        Task<ResponseModel> GetResponseAsync(RequestModel requestModel, AjaxRequestModel ajaxRequest);
    }
}