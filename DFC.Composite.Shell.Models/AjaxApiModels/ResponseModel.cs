using System.Net;

namespace DFC.Composite.Shell.Models.AjaxApiModels
{
    public class ResponseModel
    {
        public HttpStatusCode Status { get; set; }

        public string? StatusMessage { get; set; }

        public bool IsHealthy { get; set; }

        public string? OfflineHtml { get; set; }

        public string? Payload { get; set; }
    }
}
