using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Globalization;
using System.Threading.Tasks;
using UAParser;

namespace DFC.Composite.Shell.Services.Neo4J
{
    public class Neo4JService : INeo4JService
    {
        private const string NodeNameTransform = "recommendation__";
        private readonly bool sendData;
        private readonly IGraphDatabase graphDatabase;
        private readonly IServiceProvider serviceProvider;

        public Neo4JService(IOptions<Neo4JSettings> settings, IGraphDatabase graphDatabase, IServiceProvider serviceProvider)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            this.graphDatabase = graphDatabase;
            this.serviceProvider = serviceProvider;

            sendData = settings.Value.SendData;
        }

        public Task InsertNewRequest(HttpRequest request)
        {
            if (!sendData)
            {
                return Task.CompletedTask;
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return InsertNewRequestInternal(request);
        }

        private static Guid GetSessionId(HttpRequest request)
        {
            const string compositeSessionIdHeaderName = "ncs_session_cookie";

            if (request != null && request.Cookies.TryGetValue(compositeSessionIdHeaderName, out var headerValue) && Guid.TryParse(headerValue, out var guidValue))
            {
                return guidValue;
            }

            return default;
        }

        private static string GenerateVisitKey(Guid userId, string page)
        {
            string newGuid = Guid.NewGuid().ToString("D");

            return $"{userId}{page}/{newGuid}";
        }

        private static string GetReferer(HttpRequest request)
        {
            var refererHeader = request.Headers["Referer"].ToString();

            return string.IsNullOrEmpty(refererHeader) ? string.Empty : new Uri(refererHeader).LocalPath;
        }

        private async Task InsertNewRequestInternal(HttpRequest request)
        {
            var parser = Parser.GetDefault();
            var userAgent = parser.Parse(request.Headers["User-agent"].ToString());

            var sessionId = GetSessionId(request);
            var visitId = GenerateVisitKey(sessionId, request.Path);

            var customCommand = serviceProvider.GetRequiredService<ICustomCommand>();

            customCommand.Command =
                $"merge (u:{NodeNameTransform}user {{id: '{sessionId}', browser: '{userAgent.UA.Family}', device:'{userAgent.Device}', OS: '{userAgent.OS}'}})" +
                $"\r\nmerge (v:{NodeNameTransform}visit {{visitId: '{visitId}', dateofvisit:'{DateTime.UtcNow}', referrer: \"{GetReferer(request)}\" }})" +
                $"\r\nmerge (p:{NodeNameTransform}page {{id:\"{request.Path}\"}})" +
                "\r\nwith u,v,p" +
                "\r\ncreate (v)-[:PageAccessed]->(p)" +
                "\r\ncreate (u)-[:visited]->(v)" +
                "\r\nwith u,v,p " +
                $"\r\nmatch(a:{NodeNameTransform}user)-[:visited]-(parent)-[:PageAccessed]-(page)" +
                $"\r\nwhere a.id = '{sessionId}' and parent.visitId <> '{visitId}'" +
                "\r\nwith u,v,p,parent,page \r\n" +
                "order by parent.dateOfVisit DESC limit 1" +
                "\r\nForeach (i in case when page.id = v.referrer then [1] else [] end |\r\ncreate (v)-[:hasReferrer]->(parent)\r\n)";

            await graphDatabase.Run(customCommand).ConfigureAwait(false);
        }
    }
}
