using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ResortLagoon.Domain.Entities;
using ResortLagoon.Infrastructure.Data;
using ResortLagoon.Web.ViewModels;

namespace ResortLagoon.Web.Controllers
{
    public class VillaNumberController(ApplicationDbContext _db) : Controller
    {
        public IActionResult Index()
        {
            var villaNumber = _db.villaNumbers.Include(u => u.Villa).ToList();
            return View(villaNumber);
        }

        public IActionResult Create()
        {
            VillaNumberVM villaList = new()
            {
                VillaList = _db.Villas.ToList()
                .Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                })
            };
            return View(villaList);
        }

        [HttpPost]
        public IActionResult Create(VillaNumberVM obj)
        {
            bool isRoomNumberExist = _db.villaNumbers.Any(u => u.Villa_Number == obj.VillaNumber.Villa_Number);

            if (ModelState.IsValid && !isRoomNumberExist)
            {
                _db.villaNumbers.Add(obj.VillaNumber);
                _db.SaveChanges();
                TempData["success"] = "The villa number has been created successfully.";
                return RedirectToAction("Index");
            }

            if(isRoomNumberExist)
            {
                TempData["error"] = "The villa number already exists.";

            }

            obj.VillaList = _db.Villas.ToList()
                .Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });

            return View(obj);
        }

        public IActionResult Update(int id)
        {
            Villa? obj = _db.Villas.FirstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return RedirectToAction("Error", "Home");
                //return NotFound();

            }
            return View(obj);
        }

        [HttpPost]
        public IActionResult Update(Villa obj)
        {
            if (ModelState.IsValid && obj.Id > 0)
            {
                _db.Villas.Update(obj);
                _db.SaveChanges();
                TempData["success"] = "The villa has been updated successfully.";
                return RedirectToAction("Index");
            }
            return View();
        }

        public IActionResult Delete(int id)
        {
            Villa? obj = _db.Villas.FirstOrDefault(u => u.Id == id);
            if (obj is null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(obj);
        }

        [HttpPost]
        public IActionResult Delete(Villa obj)
        {
            Villa? objFromDb = _db.Villas.FirstOrDefault(u => u.Id == obj.Id);
            if (objFromDb is not null)
            {
                _db.Villas.Remove(objFromDb);
                _db.SaveChanges();
                TempData["success"] = "The villa has been deleted successfully.";
                return RedirectToAction("Index");
            }
            return View();
        }
    }
}
