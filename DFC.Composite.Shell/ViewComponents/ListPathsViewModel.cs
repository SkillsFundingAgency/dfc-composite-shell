using DFC.Composite.Shell.Models.AppRegistrationModels;
using System.Collections.Generic;

namespace DFC.Composite.Shell.ViewComponents
{
    public class ListPathsViewModel
    {
        public IEnumerable<AppRegistrationModel> Paths { get; set; }
    }
}