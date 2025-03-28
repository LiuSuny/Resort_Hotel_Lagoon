﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ResortLagoon.Domain.Entities;
using ResortLagoon.Infrastructure.Data;

namespace ResortLagoon.Web.Controllers
{
    public class VillaNumberController(ApplicationDbContext _db) : Controller
    {
        public IActionResult Index()
        {
            var villaNumber = _db.villaNumbers.ToList();
            return View(villaNumber);
        }

        public IActionResult Create()
        {
            //use projection to display existing villa in view 
            IEnumerable<SelectListItem> list = 
                _db.Villas.ToList()
                .Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            });
            return View();
        }

        [HttpPost]
        public IActionResult Create(VillaNumber villaNumber)
        {
            //ModelState.Remove("villa");
            if (ModelState.IsValid)
            {
                _db.villaNumbers.Add(villaNumber);
                _db.SaveChanges();
                TempData["success"] = "The villa number has been created successfully.";
                return RedirectToAction("Index");
            }
            return View();
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
