using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResortLagoon.Application.Common.Interfaces;
using ResortLagoon.Application.Common.Utilities;
using ResortLagoon.Domain.Entities;
using Stripe;
using Stripe.Checkout;
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
        public IActionResult Index()
        {
            return View();
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

            var domain = Request.Scheme+"://"+Request.Host.Value+"/";
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain + $"booking/BookingConfirmation?bookingId={booking.Id}",
                CancelUrl = domain + $"booking/FinalizeBooking?villaId={booking.VillaId}&checkInDate={booking.CheckInDate}&nights={booking.Nights}",
            };
            options.LineItems.Add(new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(booking.TotalCost * 100), // Stripe expects the amount in cents
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = villa.Name,
                        //Images = new List<string> {domain + villa.ImageUrl },
                        //Description = $"Check-in: {booking.CheckInDate}, Nights: {booking.Nights}"
                    }
                },
                Quantity = 1,
            });
            
            var service = new SessionService();
            Session session = service.Create(options);
             
            _unitOfWork.Booking.UpdateStripePaymentID(booking.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);


            //return RedirectToAction(nameof(BookingConfirmation),new {bookingId = booking.Id  } );
        }

        [Authorize]
        public IActionResult BookingConfirmation(int bookingId)
        {
            Booking bookingFromDb = _unitOfWork.Booking.Get(b => b.Id == bookingId, 
                includeProperties: "User,Villa");

            if(bookingFromDb.Status == SD.StatusPending)
            {
                //This is pending booking, we need to check with Stripe if the payment is successful or not
                var service = new SessionService();
                Session session = service.Get(bookingFromDb.StripeSessionId);
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.Booking.UpdateStatus(bookingFromDb.Id, SD.StatusApproved);
                    _unitOfWork.Booking.UpdateStripePaymentID(bookingFromDb.Id, session.Id, session.PaymentIntentId);
                    _unitOfWork.Save();
                }
            }
            return View(bookingId);
        }

     
        #region API Calls
        [HttpGet]
        [Authorize]
        public IActionResult GetAll()
        {
            // Get the current user's ID
            IEnumerable<Booking> objBookings;
            // Check if the user is in the Admin role
            if (User.IsInRole(SD.Role_Admin))
            {
                // If the user is an admin, retrieve all bookings with related User and Villa data
                objBookings = _unitOfWork.Booking.GetAll(includeProperties: "User,Villa");
            }
            else
            {
                // If the user is not an admin, retrieve only the bookings associated with the current user's ID, including related User and Villa data
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                // Retrieve bookings for the current user with related User and Villa data
                objBookings = _unitOfWork.Booking
                    .GetAll(u => u.UserId == userId, includeProperties: "User,Villa");
            }
            // Return the bookings as JSON data
            return Json(new { data = objBookings });
        }

        #endregion
    }
}
