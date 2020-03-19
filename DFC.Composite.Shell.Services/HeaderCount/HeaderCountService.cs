using DFC.Composite.Shell.Models.Common;

namespace DFC.Composite.Shell.Services.HeaderCount
{
    public class HeaderCountService : IHeaderCountService
    {
        public int Count(string headerName)
        {
            var result = int.MaxValue;

            if (!string.IsNullOrWhiteSpace(headerName) && headerName == Constants.DfcSession)
            {
                result = 1;
            }

            return result;
        }
    }
}
