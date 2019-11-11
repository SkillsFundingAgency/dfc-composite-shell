using System;

namespace DFC.Composite.Shell.Services.UrlRewriter
{
    public class UrlRewriterService : IUrlRewriterService
    {
        public string Rewrite(string content, string requestBaseUrl, string applicationRootUrl)
        {
            var attributeNames = new string[] { "href", "action" };
            var quoteChars = new char[] { '"', '\'' };

            foreach (var attributeName in attributeNames)
            {
                foreach (var quoteChar in quoteChars)
                {
                    var fromUrlPrefixes = new string[] { $@"{attributeName}={quoteChar}/", $@"{attributeName}={quoteChar}{applicationRootUrl}/" };
                    var toUrlPrefix = $@"{attributeName}={quoteChar}{requestBaseUrl}/";

                    foreach (var fromUrlPrefix in fromUrlPrefixes)
                    {
                        if (content.Contains(fromUrlPrefix, StringComparison.InvariantCultureIgnoreCase))
                        {
                            content = content.Replace(fromUrlPrefix, toUrlPrefix, StringComparison.InvariantCultureIgnoreCase);
                        }
                    }
                }
            }

            return content;
        }
    }
}