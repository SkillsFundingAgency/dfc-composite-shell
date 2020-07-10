using System;

namespace DFC.Composite.Shell.Services.Neo4J
{
    public class VisitRequestModel
    {
        public Guid SessionId { get; set; }

        public string Referer { get; set; }

        public string UserAgent { get; set; }

        public string RequestPath { get; set; }

        public DateTime VisitTime { get; set; }
    }
}
