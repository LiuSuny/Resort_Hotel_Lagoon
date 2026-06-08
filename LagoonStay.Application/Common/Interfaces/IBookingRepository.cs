using LagoonStay.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoonStay.Application.Common.Interfaces
{
    public interface IBookingRepository : IRepository<Booking>
    {
        void Update(Booking entity);
        //add a method to update the order booking status 
        void UpdateStatus(int bookingId, string bookingStatus, int villaNumber);
        void UpdateStripePaymentID(int bookingId, string sessionId, string paymentIntentId);

    }
}
