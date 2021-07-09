using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using RazorEngine.Templating;

namespace DFC.Composite.Shell.Views.Test.Services.ViewRenderer
{
    public class HtmlSupportTemplateBase<T> : TemplateBase<T>
    {
        public HtmlSupportTemplateBase()
        {
            Html = new RazorHtmlHelper();
            Component = A.Fake<IViewComponentHelper>();
        }

        public RazorHtmlHelper Html { get; set; }
        public IViewComponentHelper Component { get; set; }

        public void IgnoreSection(string _) { }
    }
}
