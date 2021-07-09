using DFC.Composite.Shell.Models.Common;

namespace DFC.Composite.Shell.Services.HeaderCount
{
    public class HeaderCountService : IHeaderCountService
    {
        public int Count(string headerName)
        {
            var isDfcSession = headerName == Constants.DfcSession;
            return isDfcSession ? 1 : int.MaxValue;
        }
    }
}
