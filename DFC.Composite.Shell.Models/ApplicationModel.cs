using DFC.Composite.Shell.Models.AppRegistration;
using System;

namespace DFC.Composite.Shell.Models
{
    public class ApplicationModel
    {
        public AppRegistrationModel AppRegistrationModel { get; set; }

        public Uri RootUrl { get; set; }

        public string Article { get; set; }
    }
}