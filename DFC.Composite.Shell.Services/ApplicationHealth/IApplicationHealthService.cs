using DFC.Composite.Shell.Models.HealthModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.ApplicationHealth
{
    public interface IApplicationHealthService
    {
        Task<IEnumerable<HealthItemModel>> GetAsync(ApplicationHealthModel model);
    }
}
