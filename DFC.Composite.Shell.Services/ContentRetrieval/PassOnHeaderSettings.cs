using System.Collections.Generic;

namespace DFC.Composite.Shell.Services.ContentRetrieval
{
    public class PassOnHeaderSettings
    {
        public PassOnHeaderSettings()
        {
            SupportedHeaders = new List<string>();
        }

        public List<string> SupportedHeaders { get; set; }
    }
}
