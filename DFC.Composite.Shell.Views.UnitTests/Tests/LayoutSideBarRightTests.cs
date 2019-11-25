using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Views.Test.Services.ViewRenderer;
using Microsoft.AspNetCore.Html;
using System.Collections.Generic;
using Xunit;

namespace DFC.Composite.Shell.Views.Test.Tests
{
    public class LayoutSideBarRightTests : TestBase
    {
        private string _layout = "_LayoutSideBarRight";

        [Fact]
        public void SideBarLeftDoesNotAppear()
        {
            var model = new PageViewModelResponse();
            model.LayoutName = _layout;
            model.ContentSidebarLeft = new HtmlString("ContentSideBarLeft");
            var viewBag = new Dictionary<string, object>();
            var viewRenderer = new RazorEngineRenderer(ViewRootPath);

            var viewRenderResponse = viewRenderer.Render(@"RenderView", model, viewBag);

            Assert.DoesNotContain(model.ContentSidebarLeft.Value, viewRenderResponse);
        }

        [Fact]
        public void ContainsContentFromOtherSections()
        {
            var model = new PageViewModelResponse();
            model.LayoutName = _layout;
            model.ContentHead = new HtmlString("ContentHead");
            model.ContentBodyTop = new HtmlString("ContentBodyTop");
            model.ContentBreadcrumb = new HtmlString("ContentBreadcrumb");
            model.ContentBody = new HtmlString("ContentBody");
            model.ContentBodyFooter = new HtmlString("ContentBodyFooter");
            model.ContentSidebarRight = new HtmlString("ContentSideBarRight");
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
