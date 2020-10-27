using Microsoft.AspNetCore.Http;

namespace DFC.Composite.Shell.Models
{
    public class ActionPostRequestModel : ActionGetRequestModel
    {
        public IFormCollection FormCollection { get; set; }
    }
}