using Microsoft.AspNetCore.Mvc;

namespace LagoonStay.Web.Controllers
{
    public class PlayingController : Controller
    {

        public IActionResult Edit(string id)
        {
            return View(id);
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
