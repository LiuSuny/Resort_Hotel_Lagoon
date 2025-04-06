using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ResortLagoon.Application.Common.Interfaces;
using ResortLagoon.Domain.Entities;
using ResortLagoon.Infrastructure.Data;
using ResortLagoon.Web.ViewModels;

namespace ResortLagoon.Web.Controllers
{
    public class VillaNumberController(IUnitOfWork _unitOfWork) : Controller
    {
        public IActionResult Index()
        {
            var villaNumber = _unitOfWork.VillaNumber.GetAll(includeProperties: "Villa").ToList();
            return View(villaNumber);
        }

        public IActionResult Create()
        {
            VillaNumberVM villaList = new()
            {
                VillaList = _unitOfWork.Villa.GetAll()
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
            bool isRoomNumberExist = _unitOfWork.VillaNumber.Any(u => u.Villa_Number == obj.VillaNumber.Villa_Number);

            if (ModelState.IsValid && !isRoomNumberExist)
            {
                _unitOfWork.VillaNumber.Add(obj.VillaNumber);
                _unitOfWork.Save();
                TempData["success"] = "The villa number has been created successfully.";
                return RedirectToAction(nameof(Index));
            }

            if(isRoomNumberExist)
            {
                TempData["error"] = "The villa number already exists.";

            }

            obj.VillaList = _unitOfWork.Villa.GetAll()
                .Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });

            return View(obj);
        }

        public IActionResult Update(int villaNumberId)
        {
            VillaNumberVM villaListVM = new()
            {
                VillaList = _unitOfWork.Villa.GetAll()
                 .Select(u => new SelectListItem
                 {
                     Text = u.Name,
                     Value = u.Id.ToString()
                 }),

                VillaNumber = _unitOfWork.VillaNumber.Get(u => u.Villa_Number == villaNumberId)
            };

            if (villaListVM.VillaNumber == null)
            {
                return RedirectToAction("Error", "Home");
                //return NotFound();

            }
            return View(villaListVM);
        }

        [HttpPost]
        public IActionResult Update(VillaNumberVM villaNumberVM)
        {
           

            if (ModelState.IsValid)
            {
                _unitOfWork.VillaNumber.Update(villaNumberVM.VillaNumber);
                _unitOfWork.Save();
                TempData["success"] = "The villa number has been updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            villaNumberVM.VillaList = _unitOfWork.Villa.GetAll()
                .Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });

            return View(villaNumberVM);
        }

        public IActionResult Delete(int villaNumberId)
        {
            VillaNumberVM villaListVM = new()
            {
                VillaList = _unitOfWork.Villa.GetAll()
                 .Select(u => new SelectListItem
                 {
                     Text = u.Name,
                     Value = u.Id.ToString()
                 }),

                VillaNumber = _unitOfWork.VillaNumber.Get(u => u.Villa_Number == villaNumberId)
            };

            if (villaListVM.VillaNumber == null)
            {
                return RedirectToAction("Error", "Home");              

            }
            return View(villaListVM);
        }

        [HttpPost]
        public IActionResult Delete(VillaNumberVM villaNumberVM)
        {
            VillaNumber? objFromDb = _unitOfWork.VillaNumber.Get
                 (u => u.Villa_Number == villaNumberVM.VillaNumber.Villa_Number);
            if (objFromDb is not null)
            {
                _unitOfWork.VillaNumber.Remove(objFromDb);
                _unitOfWork.Save();
                TempData["success"] = "The villa number has been deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "The villa number could not be deleted.";
            return View();
        }
    }
}
