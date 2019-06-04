using DFC.Composite.Shell.Services.PathLocator;
using HtmlAgilityPack;
using System;

namespace DFC.Composite.Shell.Services.UrlRewriter
{
    public class UrlRewriter : IUrlRewriter
    {
        private readonly IPathLocator _pathLocator;

        public UrlRewriter(IPathLocator pathLocator)
        {
            _pathLocator = pathLocator;
        }

        public string Rewrite(string content)
        {
            //var htmlDoc = new HtmlDocument();
            //htmlDoc.LoadHtml(content);

            //var links = htmlDoc.DocumentNode.SelectNodes("//a");
            //if (links != null)
            //{
            //    //foreach (var link in links)
            //    //{
            //    //    var href = link.Attributes["href"];
            //    //    if (href != null && IsRelativeUrl(href.Value))
            //    //    {
            //    //        href.Value = $"{_pathLocator.GetPath()}?route={href.Value}";
            //    //    }
            //    //}

            //    content = htmlDoc.DocumentNode.InnerHtml;
            //}

            return content;
        }

        private bool IsRelativeUrl(string url)
        {
            var uri = new Uri(url, UriKind.RelativeOrAbsolute);
            return !uri.IsAbsoluteUri;
        }
    }
}
