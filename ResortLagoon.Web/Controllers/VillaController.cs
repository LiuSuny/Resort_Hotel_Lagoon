using Microsoft.AspNetCore.Mvc;
using ResortLagoon.Application.Common.Interfaces;
using ResortLagoon.Domain.Entities;
using ResortLagoon.Infrastructure.Data;

namespace ResortLagoon.Web.Controllers
{
    public class VillaController(IUnitOfWork _unitOfWork) : Controller
    {
        public IActionResult Index()
        {
            var villas = _unitOfWork.Villa.GetAll();
            return View(villas);
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
                _unitOfWork.Villa.Add(villa);
                _unitOfWork.Save();
                TempData["success"] = "The villa has been created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        public IActionResult Update(int id)
        {
            Villa? obj = _unitOfWork.Villa.Get(u => u.Id == id);
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
                _unitOfWork.Villa.Update(obj);
                _unitOfWork.Save();
                TempData["success"] = "The villa has been updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        public IActionResult Delete(int id)
        {
            Villa? obj = _unitOfWork.Villa.Get(u => u.Id == id);
            if (obj is null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(obj);
        }

        [HttpPost]
        public IActionResult Delete(Villa obj)
        {
            Villa? objFromDb = _unitOfWork.Villa.Get(u => u.Id == obj.Id);
            if (objFromDb is not null)
            {
                _unitOfWork.Villa.Remove(objFromDb);
                _unitOfWork.Save();
                TempData["success"] = "The villa has been deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View();
        }
    }
}
