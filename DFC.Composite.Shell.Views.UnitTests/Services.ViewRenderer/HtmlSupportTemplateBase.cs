using RazorEngine.Templating;

namespace DFC.Composite.Shell.Views.Test.Services.ViewRenderer
{
    public class HtmlSupportTemplateBase<T> : TemplateBase<T>
    {
        public HtmlSupportTemplateBase()
        {
            Html = new RazorHtmlHelper();
        }

        public RazorHtmlHelper Html { get; set; }

        public void IgnoreSection(string sectionName)
        {

        }
    }
}
