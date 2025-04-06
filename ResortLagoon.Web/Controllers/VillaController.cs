using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using ResortLagoon.Application.Common.Interfaces;
using ResortLagoon.Domain.Entities;
using ResortLagoon.Infrastructure.Data;

namespace ResortLagoon.Web.Controllers
{
    public class VillaController(IUnitOfWork _unitOfWork, IWebHostEnvironment _webHost) : Controller
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
                if(villa.Image != null)
                {
                    //retrive with random guid with upload image
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(villa.Image.FileName);
                    // path root
                    string imagePath = Path.Combine(_webHost.WebRootPath, @"images\VillaImage");

                    //create and joing the images and copy it to villa folder in www.root fold
                    using var fileStream = new FileStream(Path.Combine(imagePath, fileName), FileMode.Create);
                    villa.Image.CopyTo(fileStream);

                    villa.ImageUrl = @"\images\VillaImage\" + fileName;
                }
                else
                {
                    villa.ImageUrl = "https://placehold.co/600x400";
                }
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
                if (obj.Image != null)
                {
                    //retrive with random guid with upload image
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(obj.Image.FileName);
                    // path root
                    string imagePath = Path.Combine(_webHost.WebRootPath, @"images\VillaImage");

                    if (!string.IsNullOrEmpty(obj.ImageUrl)) //if there is existing image and need changes
                    {
                        var oldImagePath = Path.Combine(_webHost.WebRootPath, obj.ImageUrl.TrimStart('\\'));

                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    //create and joing the images and copy it to villa folder in www.root fold
                    using var fileStream = new FileStream(Path.Combine(imagePath, fileName), FileMode.Create);
                    obj.Image.CopyTo(fileStream);

                    obj.ImageUrl = @"\images\VillaImage\" + fileName;
                }
               
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

                if (!string.IsNullOrEmpty(objFromDb.ImageUrl)) //if there is existing image and need changes
                {
                    var oldImagePath = Path.Combine(_webHost.WebRootPath, objFromDb.ImageUrl.TrimStart('\\'));

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }
                _unitOfWork.Villa.Remove(objFromDb);
                _unitOfWork.Save();
                TempData["success"] = "The villa has been deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View();
        }
    }
}
