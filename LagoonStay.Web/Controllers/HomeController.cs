using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Migrations;
using LagoonStay.Application.Common.Interfaces;
using LagoonStay.Web.Models;
using LagoonStay.Web.ViewModels;
using System;
using System.Diagnostics;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using LagoonStay.Application.Common.Utilities;

namespace LagoonStay.Web.Controllers
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


        //[HttpPost]
        //public IActionResult Index(HomeVM homeVM)
        //{
        //    homeVM.VillaList = _unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity");
        //     foreach (var villa in homeVM.VillaList)
        //    {
        //        if (villa.Id % 2 == 0)
        //        {
        //            villa.IsAvailable = false;
        //        }

        //    }
        //        return View(homeVM);
        //}

        [HttpPost]
        public IActionResult GetVillasByDate(int nights, DateOnly checkInDate)
        {
            //Thread.Sleep(2000);
            var villaList = _unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity").ToList();

            var villaNumbersList = _unitOfWork.VillaNumber.GetAll().ToList();
            var bookingvillas = _unitOfWork.Booking.GetAll(u => u.Status == SD.StatusApproved ||
            u.Status == SD.StatusCheckedIn).ToList();

            foreach (var villa in villaList)
            {

                int roomAvailable = SD.VillaRoomsAvailabile_Count(villa.Id, villaNumbersList,
                    checkInDate, nights, bookingvillas);
                villa.IsAvailable = roomAvailable > 0 ? true : false;
                //if (villa.Id % 2 == 0)
                //{
                //    villa.IsAvailable = false;
                //}
            }
            HomeVM homeVM = new()
            {
                CheckInDate = checkInDate,
                VillaList = villaList,
                Nights = nights
            };

            return PartialView("_VillaList", homeVM);
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
