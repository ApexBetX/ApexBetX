using Microsoft.AspNetCore.Mvc;

namespace ApexBetX.Controllers
{
    public class UsersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
