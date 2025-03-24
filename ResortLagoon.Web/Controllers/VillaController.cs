using Microsoft.AspNetCore.Mvc;
using ResortLagoon.Domain.Entities;
using ResortLagoon.Infrastructure.Data;

namespace ResortLagoon.Web.Controllers
{
    public class VillaController(ApplicationDbContext _db) : Controller
    {
        public IActionResult Index()
        {
            var villa = _db.Villas.ToList();
            return View(villa);
        }

        public IActionResult Create()
        {            
            return View();
        }

        [HttpPost]
        public IActionResult Create(Villa villa)
        {
            if (villa.Name == villa.Description)
            {
                ModelState.AddModelError("name", "The description cannot exactly match the Name.");
            }
            if (ModelState.IsValid)
            {
                _db.Villas.Add(villa);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View();
        }
    }
}
