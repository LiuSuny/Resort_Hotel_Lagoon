using LagoonStay.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoonStay.Application.Common.Utilities
{
    public static class SD
    {
       public const string Role_Admin = "Admin";
       public const string Role_Customer = "Customer";

       public const string StatusPending = "Pending";
       public const string StatusApproved = "Approved";
       public const string StatusCheckedIn = "CheckedIn";
       public const string StatusCompleted = "Completed";
       public const string StatusCancelled = "Cancelled";
       public const string StatusRefunded = "Refunded";

       public static int VillaRoomsAvailabile_Count(int villaId, 
           List<VillaNumber> villaNumbersList, DateOnly checkInDate, int nights, 
           List<Booking> bookings)
        {
            List<int> bookingInDates = new();

            int finalAvailableRoomsForAllNights = int.MaxValue;

            var roomsInVilla = villaNumbersList.Where(x => x.VillaId == villaId).Count();

            for (int i = 0; i < nights; i++)
            {
                var villaBooked = bookings.Where(x => x.CheckInDate
                == checkInDate.AddDays(i) && x.CheckOutDate >
                checkInDate.AddDays(i) && x.VillaId == villaId).ToList();
                foreach (var booking in villaBooked)
                {
                    if (!bookingInDates.Contains(booking.Id)) 
                    {
                        bookingInDates.Add(booking.Id);
                    }

                }
                var totalAvailableRooms = roomsInVilla - bookingInDates.Count();
                if (totalAvailableRooms == 0)
                {
                    return 0;
                }
                else
                {
                    if (finalAvailableRoomsForAllNights > totalAvailableRooms)
                    {
                        finalAvailableRoomsForAllNights = totalAvailableRooms;
                    }
                }
            }
            return finalAvailableRoomsForAllNights;
        }
    }
}
