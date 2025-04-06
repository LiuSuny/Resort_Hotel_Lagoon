using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ResortLagoon.Application.Common.Interfaces;
using ResortLagoon.Web.Models;
using ResortLagoon.Web.ViewModels;

namespace ResortLagoon.Web.Controllers
{
    public class HomeController(IUnitOfWork _unitOfWork) : Controller
    {
       

        public IActionResult Index()
        {
            HomeVM homeVM = new()
            {
                VillaList = _unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity"),
                Nights = 1,
                CheckInDate = DateOnly.FromDateTime(DateTime.Now),
                //CheckOutDate = DateOnly.FromDateTime(DateTime)
            };

           
            return View(homeVM);
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult Error()
        {         
               return View();
        }
    }
}
