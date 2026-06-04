using ResortLagoon.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResortLagoon.Application.Common.Interfaces
{
    public interface IBookingRepository : IRepository<Booking>
    {
        void Update(Booking entity);
        //add a method to update the order booking status 
        void UpdateStatus(int bookingId, string bookingStatus);
        void UpdateStripePaymentID(int bookingId, string sessionId, string paymentIntentId);

    }
}
