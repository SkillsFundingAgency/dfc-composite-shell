using System;

namespace DFC.Composite.Shell.Policies.Options
{
    public class HttpClientOptions
    {
        public Uri BaseAddress { get; set; }

        public TimeSpan Timeout { get; set; } = new TimeSpan(0, 0, 30);         // default to 30 seconds
    }
}
