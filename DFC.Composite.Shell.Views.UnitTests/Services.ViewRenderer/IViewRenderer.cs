using System.Collections.Generic;

namespace DFC.Composite.Shell.Views.Test.Services.ViewRenderer
{
    public interface IViewRenderer
    {
        string Render(string template, object model, IDictionary<string, object> viewBag);
    }
}
