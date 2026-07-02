using Microsoft.AspNetCore.Mvc;

namespace ApexBetX.Controllers
{
    public class TestController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
