using DFC.Composite.Shell.Models.Common;

namespace DFC.Composite.Shell.Services.HeaderRenamer
{
    public class HeaderRenamerService : IHeaderRenamerService
    {
        public bool Rename(string headerName)
        {
            return headerName != Constants.DfcSession;
        }
    }
}