using DFC.Composite.Shell.Models.AppRegistration;
using System.Collections.Generic;

namespace DFC.Composite.Shell.ViewComponents
{
    public class ListPathsViewModel
    {
        public IEnumerable<AppRegistrationModel> AppRegistrationModels { get; set; }
    }
}
