using LagoonStay.Application.Common.Interfaces;
using LagoonStay.Application.Common.Utilities;
using LagoonStay.Web.Models;
using LagoonStay.Web.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Migrations;
using Syncfusion.DocIO.DLS;
using Syncfusion.Presentation;
using System;
using System.ComponentModel;
using System.Diagnostics;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace LagoonStay.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

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


        [HttpPost]
        public IActionResult GeneratePPTExport(int id)
        {
            //fetch villa details
            var villa = _unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity").FirstOrDefault(x => x.Id == id);
            
            //redirect if villa is 0 
            if (villa is null)
            {
                return RedirectToAction(nameof(Error));
            }

            //creating a base path where to fetch file from 
            string basePath = _webHostEnvironment.WebRootPath;
            string filePath = basePath + @"/Exports/ExportVillaDetails.pptx";

            
            using IPresentation presentation = Presentation.Open(filePath);

            ISlide slide = presentation.Slides[0];


            IShape? shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "txtVillaName") as IShape;
            if (shape is not null)
            {
                shape.TextBody.Text = villa.Name;
            }

            shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "txtVillaDescription") as IShape;
            if (shape is not null)
            {
                shape.TextBody.Text = villa.Description;
            }


            shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "txtOccupancy") as IShape;
            if (shape is not null)
            {
                shape.TextBody.Text = string.Format("Max Occupancy : {0} adults", villa.Occupancy);
            }
            shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "txtVillaSize") as IShape;
            if (shape is not null)
            {
                shape.TextBody.Text = string.Format("Villa Size: {0} sqft", villa.Sqft);
            }
            shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "txtPricePerNight") as IShape;
            if (shape is not null)
            {
                shape.TextBody.Text = string.Format("USD {0}/night", villa.Price.ToString("C"));
            }

            shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "txtVillaAmenitiesHeading") as IShape;
            if (shape is not null)
            {
                List<string> listItems = villa.VillaAmenity.Select(x => x.Name).ToList();

                shape.TextBody.Text = "";

                foreach (var item in listItems)
                {
                    IParagraph paragraph = shape.TextBody.AddParagraph();
                    ITextPart textPart = paragraph.AddTextPart(item);

                    paragraph.ListFormat.Type = Syncfusion.Presentation.ListType.Bulleted;
                    paragraph.ListFormat.BulletCharacter = '\u2022';
                    textPart.Font.FontName = "system-ui";
                    textPart.Font.FontSize = 18;
                    textPart.Font.Color = ColorObject.FromArgb(144, 148, 152);

                }

            }

            //Images config
            shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "imgVilla") as IShape;
            if (shape is not null)
            {
                byte[] imageData;
                string imageUrl;
                try
                {
                    imageUrl = string.Format("{0}{1}", basePath, villa.ImageUrl);
                    imageData = System.IO.File.ReadAllBytes(imageUrl);
                }
                catch (Exception)
                {
                    imageUrl = string.Format("{0}{1}", basePath, "/images/placeholder.png");
                    imageData = System.IO.File.ReadAllBytes(imageUrl);
                }
                slide.Shapes.Remove(shape);
                using MemoryStream imageStream = new(imageData);
                IPicture newPicture = slide.Pictures.AddPicture(imageStream, 60, 120, 300, 200);

            }

            MemoryStream memoryStream = new();
            presentation.Save(memoryStream);
            memoryStream.Position = 0;
            return File(memoryStream, "application/pptx", "villa.pptx");


        }


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
