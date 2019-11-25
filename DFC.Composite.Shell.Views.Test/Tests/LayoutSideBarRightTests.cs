using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Views.Test.Services.ViewRenderer;
using Microsoft.AspNetCore.Html;
using System.Collections.Generic;
using Xunit;

namespace DFC.Composite.Shell.Views.Test.Tests
{
    public class LayoutSideBarRightTests : TestBase
    {
        private const string _layout = "_LayoutSideBarRight";

        [Fact]
        public void SideBarLeftDoesNotAppear()
        {
            var model = new PageViewModelResponse
            {
                LayoutName = _layout,
                ContentSidebarLeft = new HtmlString("ContentSideBarLeft")
            };
            var viewBag = new Dictionary<string, object>();
            var viewRenderer = new RazorEngineRenderer(ViewRootPath);

            var viewRenderResponse = viewRenderer.Render(@"RenderView", model, viewBag);

            Assert.DoesNotContain(model.ContentSidebarLeft.Value, viewRenderResponse);
        }

        [Fact]
        public void ContainsContentFromOtherSections()
        {
            var model = new PageViewModelResponse
            {
                LayoutName = _layout,
                ContentHead = new HtmlString("ContentHead"),
                ContentBodyTop = new HtmlString("ContentBodyTop"),
                ContentBreadcrumb = new HtmlString("ContentBreadcrumb"),
                ContentBody = new HtmlString("ContentBody"),
                ContentBodyFooter = new HtmlString("ContentBodyFooter"),
                ContentSidebarRight = new HtmlString("ContentSideBarRight")
            };
            var viewBag = new Dictionary<string, object>();
            var viewRenderer = new RazorEngineRenderer(ViewRootPath);

            var viewRenderResponse = viewRenderer.Render(@"RenderView", model, viewBag);

            Assert.Contains(model.ContentHead.Value, viewRenderResponse);
            Assert.Contains(model.ContentBodyTop.Value, viewRenderResponse);
            Assert.Contains(model.ContentBreadcrumb.Value, viewRenderResponse);
            Assert.Contains(model.ContentBody.Value, viewRenderResponse);
            Assert.Contains(model.ContentBodyFooter.Value, viewRenderResponse);
            Assert.Contains(model.ContentSidebarRight.Value, viewRenderResponse);
        }
    }
}
