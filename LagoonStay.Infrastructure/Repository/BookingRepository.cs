using LagoonStay.Application.Common.Interfaces;
using LagoonStay.Application.Common.Utilities;
using LagoonStay.Domain.Entities;
using LagoonStay.Infrastructure.Data;

namespace LagoonStay.Infrastructure.Repository
{
    public class BookingRepository(ApplicationDbContext _db) : Repository<Booking>(_db), IBookingRepository
    {
        public void Update(Booking entity)
        {
            _db.Bookings.Update(entity);
        }

        public void UpdateStatus(int bookingId, string bookingStatus)
        {
            //get the booking from the database
            var bookingFromDb = _db.Bookings.FirstOrDefault(u => u.Id == bookingId);
            //if the booking is not null, update the status
            if (bookingFromDb != null)
            {
                //update the status
                bookingFromDb.Status = bookingStatus;
                //if the status is checked in, update the actual check in date
                if (bookingStatus == SD.StatusCheckedIn)
                {
                    bookingFromDb.ActualCheckInDate = DateTime.Now;
                }
                //if the status is completed, update the actual check out date
                if (bookingStatus == SD.StatusCompleted)
                {
                    bookingFromDb.ActualCheckOutDate = DateTime.Now;
                }
            }
            // _db.SaveChanges();
        }

        public void UpdateStripePaymentID(int bookingId, string sessionId, string paymentIntentId)
        {
            //get the booking from the database
            var bookingFromDb = _db.Bookings.FirstOrDefault(u => u.Id == bookingId);
            //if the booking is not null, update the Stripe session ID and payment intent ID
            if (bookingFromDb != null)
            {
                //update the Stripe session ID and payment intent ID
                if (!string.IsNullOrEmpty(sessionId))
                {
                    bookingFromDb.StripeSessionId = sessionId;
                }
                //update the payment intent ID, payment date, and payment status if the payment intent ID is not null or empty
                if (!string.IsNullOrEmpty(paymentIntentId))
                {
                    bookingFromDb.StripePaymentIntentId = paymentIntentId;
                    bookingFromDb.PaymentDate = DateTime.Now;
                    bookingFromDb.IsPaymentSuccessful = true;
                }

            }
        }
    }
}
