using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResortLagoon.Application.Common.Interfaces;
using ResortLagoon.Application.Common.Utilities;
using ResortLagoon.Domain.Entities;
using System.Security.Claims;

namespace ResortLagoon.Web.Controllers
{
    public class BookingController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public BookingController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [Authorize]
        public IActionResult FinalizeBooking(int villaId, DateOnly checkInDate, int nights)
        {
            // Get the current user's ID
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            // Assuming the user is authenticated, get the user ID from the claims
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            // Retrieve the user from the database using the user ID
            ApplicationUser user = _unitOfWork.User.Get(u => u.Id == userId);

            Booking booking = new()
            {
                VillaId = villaId,
                Villa = _unitOfWork.Villa.Get(u => u.Id == villaId, includeProperties: "VillaAmenity"),
                CheckInDate = checkInDate,
                Nights = nights,
                CheckOutDate = checkInDate.AddDays(nights),
                UserId = userId,
                Phone = user.PhoneNumber,
                Email = user.Email,
                Name = user.Name

            };
            booking.TotalCost = (double)(booking.Villa.Price * nights);
            return View(booking);
        }

        [Authorize]
        [HttpPost]
        public IActionResult FinalizeBooking(Booking booking)
        {
            // Get the current user's ID
            var villa = _unitOfWork.Villa.Get(v => v.Id == booking.VillaId /*includeProperties: "VillaAmenity"*/);
            // Total cost calculation
            booking.TotalCost = (double)(villa.Price * booking.Nights);
            //Update the status of the booking
            booking.Status = SD.StatusPending;
            // Set the booking date to the current date and time
            booking.BookingDate = DateTime.Now;
            // Add the booking to the database
            _unitOfWork.Booking.Add(booking);
            // Save the changes to the database
            _unitOfWork.Save();

            return RedirectToAction(nameof(BookingConfirmation),new {bookingId = booking.Id  } );
        }

        [Authorize]
        public IActionResult BookingConfirmation(int bookingId)
        {
            return View(bookingId);
        }
    }
}
