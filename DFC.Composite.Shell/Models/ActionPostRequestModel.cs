using Microsoft.AspNetCore.Http;

namespace DFC.Composite.Shell.Models
{
    public class ActionPostRequestModel
    {
        public string Path { get; set; }
        public string Data { get; set; }
        public IFormCollection FormCollection { get; set; }
    }
}
