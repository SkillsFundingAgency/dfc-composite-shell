using System;

namespace DFC.Composite.Shell.Services.UrlRewriter
{
    public class UrlRewriterService : IUrlRewriterService
    {
        public string RewriteAttributes(string content, Uri requestBaseUrl, Uri applicationRootUrl)
        {
            var attributesThatCanBeRewritten = new string[] { "href", "action" };
            var quoteCharacters = new char[] { '"', '\'' };

            foreach (var attribute in attributesThatCanBeRewritten)
            {
                foreach (var quoteCharacter in quoteCharacters)
                {
                    var fromUrlPrefix = $"{attribute}={quoteCharacter}{applicationRootUrl}/";
                    var toUrlPrefix = $"{attribute}={quoteCharacter}{requestBaseUrl}/";

                    if (!content.Contains(fromUrlPrefix, StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }

                    content = content.Replace(fromUrlPrefix, toUrlPrefix, StringComparison.InvariantCultureIgnoreCase);
                }
            }

            return content;
        }
    }
}