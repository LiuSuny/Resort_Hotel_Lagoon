using LagoonStay.Application.Common.Interfaces;
using LagoonStay.Application.Common.Utilities;
using LagoonStay.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using Syncfusion.Drawing;
using System.Security.Claims;

namespace LagoonStay.Web.Controllers
{
    public class BookingController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BookingController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
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


            var villaNumbersList = _unitOfWork.VillaNumber.GetAll().ToList();
            var bookingvillas = _unitOfWork.Booking.GetAll(u => u.Status == SD.StatusApproved ||
            u.Status == SD.StatusCheckedIn).ToList();

                int roomAvailable = SD.VillaRoomsAvailabile_Count(villa.Id, villaNumbersList,
                    booking.CheckInDate, booking.Nights, bookingvillas);


            if (roomAvailable <= 0)
            {
                TempData["error"] = "Room has been sold out.";
                return RedirectToAction(nameof(FinalizeBooking),
                    new
                    {
                        villaId = booking.VillaId,
                        checkInDate = booking.CheckInDate,
                        nights = booking.Nights
                    });

            }
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
                    _unitOfWork.Booking.UpdateStatus(bookingFromDb.Id, SD.StatusApproved, 0);
                    _unitOfWork.Booking.UpdateStripePaymentID(bookingFromDb.Id, session.Id, session.PaymentIntentId);
                    _unitOfWork.Save();
                }
            }
            return View(bookingId);
        }

        [Authorize]
        public IActionResult BookingDetails(int bookingId)
        {
            Booking bookingFromDb = _unitOfWork.Booking.Get(b => b.Id == bookingId,
                includeProperties: "User,Villa");

            if(bookingFromDb.VillaNumber == 0 && bookingFromDb.Status == SD.StatusApproved)
            {
                //Assign villa number to the booking
                var availableVillaNumbers = AssignAvailableVillaNumberByVilla(bookingFromDb.VillaId);
                bookingFromDb.VillaNumbers = _unitOfWork.VillaNumber
                    .GetAll(v => v.VillaId == bookingFromDb.VillaId 
                    && availableVillaNumbers.Any(x=>x==v.Villa_Number)).ToList();
                
            }

            return View(bookingFromDb);
        }


        /// <summary>
        /// Medthod to generate an invoice for a booking using a Word template and return it as a downloadable file.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public IActionResult GenerateInvoice(int id)
        {
            // Get the base path of the web root directory
            string basePath = _webHostEnvironment.WebRootPath;

            // Create a new WordDocument instance to work with the Word template
            WordDocument document = new WordDocument();


            // Load the template.
            string dataPath = basePath + @"/exports/BookingDetails.docx";

            // Open the Word template file in read-only mode to avoid locking issues
            using FileStream fileStream = new(dataPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            // Open the Word document using the file stream and automatically detect the format type
            document.Open(fileStream, FormatType.Automatic);

            // Retrieve the booking details from the database using the provided booking ID, including related User and Villa data
            Booking bookingFromDb = _unitOfWork.Booking.Get(u => u.Id == id,
                            includeProperties: "User,Villa");

            // Replace placeholders in the Word template with actual booking details
            TextSelection textSelection = document.Find("xx_customer_name", false, true);

            // Get the text range of the found placeholder and replace it with the customer's name
            WTextRange textRange = textSelection.GetAsOneRange();

            // Set the text of the placeholder to the customer's name from the booking details
            textRange.Text = bookingFromDb.Name;

            // Fill the same just like the xx_customer_name
            textSelection = document.Find("xx_customer_phone", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingFromDb.Phone;

            // Fill the same just like the xx_customer_name
            textSelection = document.Find("xx_customer_email", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingFromDb.Email;

            // Fill the same just like the xx_customer_name
            textSelection = document.Find("xx_payment_date", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingFromDb.PaymentDate.ToShortDateString();

            // Fill the same just like the xx_customer_name
            textSelection = document.Find("XX_BOOKING_NUMBER", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = "BOOKING ID: " + bookingFromDb.Id;

            // Fill the same just like the xx_customer_name
            textSelection = document.Find("XX_BOOKING_DATE", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = "BOOKING DATE: "+ bookingFromDb.BookingDate.ToShortDateString();

            // Fill the same just like the xx_customer_name
            textSelection = document.Find("xx_checkin_date", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingFromDb.CheckInDate.ToShortDateString();

            // Fill the same just like the xx_customer_name
            textSelection = document.Find("xx_checkout_date", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingFromDb.CheckOutDate.ToShortDateString();

            // Fill the same just like the xx_customer_name
            textSelection = document.Find("xx_booking_total", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingFromDb.TotalCost.ToString("c");

            //creating word table
            WTable table = new(document);

            //formating tables
            table.TableFormat.Borders.LineWidth = 1f;
            table.TableFormat.Borders.Color = Color.Black;
            table.TableFormat.Paddings.Top = 7f;
            table.TableFormat.Paddings.Bottom = 7f;
            table.TableFormat.Borders.Horizontal.LineWidth = 1f;


            table.ResetCells(2, 4);

            WTableRow row0 = table.Rows[0];

            row0.Cells[0].AddParagraph().AppendText("NIGHTS");
            row0.Cells[0].Width = 80;
            row0.Cells[1].AddParagraph().AppendText("VILLA");
            row0.Cells[1].Width = 220;
            row0.Cells[2].AddParagraph().AppendText("PRICE PER NIGHT");
            row0.Cells[3].AddParagraph().AppendText("TOTAL");
            row0.Cells[3].Width = 80;

            WTableRow row1 = table.Rows[1];

            row1.Cells[0].AddParagraph().AppendText(bookingFromDb.Nights.ToString());
            row1.Cells[0].Width = 80;
            row1.Cells[1].AddParagraph().AppendText(bookingFromDb.Villa.Name);
            row1.Cells[1].Width = 220;
            row1.Cells[2].AddParagraph().AppendText((bookingFromDb.TotalCost / bookingFromDb.Nights).ToString("c"));
            row1.Cells[3].AddParagraph().AppendText(bookingFromDb.TotalCost.ToString("c"));
            row1.Cells[3].Width = 80;

            WTableStyle tableStyle = document.AddTableStyle("CustomStyle") as WTableStyle;
            tableStyle.TableProperties.RowStripe = 1;
            tableStyle.TableProperties.ColumnStripe = 2;
            tableStyle.TableProperties.Paddings.Top = 2;
            tableStyle.TableProperties.Paddings.Bottom = 1;
            tableStyle.TableProperties.Paddings.Left = 5.4f;
            tableStyle.TableProperties.Paddings.Right = 5.4f;

            ConditionalFormattingStyle firstRowStyle = tableStyle.ConditionalFormattingStyles.Add(ConditionalFormattingType.FirstRow);
            firstRowStyle.CharacterFormat.Bold = true;
            firstRowStyle.CharacterFormat.TextColor = Color.FromArgb(255, 255, 255, 255);
            firstRowStyle.CellProperties.BackColor = Color.Black;

            table.ApplyStyle("CustomStyle");

            TextBodyPart bodyPart = new(document);
            bodyPart.BodyItems.Add(table);

            document.Replace("<ADDTABLEHERE>", bodyPart, false, false);



            // Repeat the process for other placeholders in the template, replacing them with actual booking details
            using DocIORenderer renderer = new();

            // Replace the placeholder for the villa name with the actual villa name from the booking details
            MemoryStream stream = new();
            // Save the modified Word document to the memory stream in DOCX format
            document.Save(stream, FormatType.Docx);

            // Reset the position of the memory stream to the beginning before returning it as a file
            stream.Position = 0;

            // Return the modified Word document as a downloadable file with the appropriate content type and filename
            return File(stream, "application/docx", "BookingDetails.docx");

        }


        private List<int> AssignAvailableVillaNumberByVilla(int villaId)
        {
            List<int> availableVillaNumber = new();

            var villaNumbers = _unitOfWork.VillaNumber.GetAll(v => v.VillaId == villaId);

            var checkedInVilla = _unitOfWork.Booking.GetAll(v => v.VillaId == villaId 
            && v.Status ==SD.StatusCheckedIn).Select(v => v.VillaNumber);

            foreach (var villaNumber in villaNumbers)
            {
                if (!checkedInVilla.Contains(villaNumber.Villa_Number))
                {
                    availableVillaNumber.Add(villaNumber.Villa_Number);
                }
            }
            return availableVillaNumber;

        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CheckIn(Booking booking)
        {
           _unitOfWork.Booking.UpdateStatus(booking.Id, SD.StatusCheckedIn, booking.VillaNumber);
            _unitOfWork.Save();
            TempData["success"] = "Booking Updated Successfully.";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id});
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CheckOut(Booking booking)
        {
           _unitOfWork.Booking.UpdateStatus(booking.Id, SD.StatusCompleted, booking.VillaNumber);
            _unitOfWork.Save();
            TempData["success"] = "Booking Completed Successfully.";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id});
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CancelBooking(Booking booking)
        {
           _unitOfWork.Booking.UpdateStatus(booking.Id, SD.StatusCancelled, 0);
            _unitOfWork.Save();
            TempData["success"] = "Booking Cancelled Successfully.";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id});
        }

            #region API Calls
            [HttpGet]
        [Authorize]
        public IActionResult GetAll(string status)
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
            if (!string.IsNullOrEmpty(status))
            {
                objBookings = objBookings.Where(b => b.Status.ToLower().Equals(status.ToLower()));
            }
            // Return the bookings as JSON data
            return Json(new { data = objBookings });
        }

        #endregion
    }
}
