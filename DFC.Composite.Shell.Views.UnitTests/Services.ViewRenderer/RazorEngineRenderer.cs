using RazorEngine.Configuration;
using RazorEngine.Templating;
using System.Collections.Generic;
using System.IO;

namespace DFC.Composite.Shell.Views.Test.Services.ViewRenderer
{
    public class RazorEngineRenderer : IViewRenderer
    {
        private readonly string _viewRootPath;

        public RazorEngineRenderer(string viewRootPath)
        {
            _viewRootPath = viewRootPath;
        }

        public string Render(string viewName, object model, IDictionary<string, object> viewBag)
        {
            var razorConfig = new TemplateServiceConfiguration
            {
                TemplateManager = CreateTemplateManager(),
                BaseTemplateType = typeof(HtmlSupportTemplateBase<>)
            };

            var razorEngine = RazorEngineService.Create(razorConfig);

            var dynamicViewBag = new DynamicViewBag(viewBag);
            var result = razorEngine.RunCompile(viewName, model.GetType(), model, dynamicViewBag);

            return result;
        }

        private ITemplateManager CreateTemplateManager()
        {
            var directories = Directory.GetDirectories(_viewRootPath, "*.*", SearchOption.AllDirectories);
            return new ResolvePathTemplateManager(directories);
        }
    }
}
