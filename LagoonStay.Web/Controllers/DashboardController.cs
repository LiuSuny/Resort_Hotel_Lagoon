using Microsoft.AspNetCore.Mvc;

namespace LagoonStay.Web.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
