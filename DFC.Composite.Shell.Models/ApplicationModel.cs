using DFC.Composite.Shell.Models.AppRegistrationModels;
using System.Collections.Generic;

namespace DFC.Composite.Shell.Models
{
    public class ApplicationModel
    {
        public AppRegistrationModel AppRegistrationModel { get; set; }

        public string RootUrl { get; set; }

        public List<RegionModel> Regions
        {
            get
            {
                //TODO: ian remove this property
                return AppRegistrationModel?.Regions;
            }
        }
    }
}