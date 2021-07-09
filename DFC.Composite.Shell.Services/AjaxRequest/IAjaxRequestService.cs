using DFC.Composite.Shell.Models.AjaxApi;
using DFC.Composite.Shell.Models.AppRegistration;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.AjaxRequest
{
    public interface IAjaxRequestService
    {
        Task<ResponseModel> GetResponseAsync(RequestModel requestModel, AjaxRequestModel ajaxRequest);
    }
}