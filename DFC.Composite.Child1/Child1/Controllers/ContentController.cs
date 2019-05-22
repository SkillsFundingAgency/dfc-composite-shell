using Microsoft.AspNetCore.Mvc;

namespace Child1.Controllers
{
    public class ContentController : Controller
    {
        public IActionResult Footer()
        {
            return Content("Hello from footer");
        }
    }
}
