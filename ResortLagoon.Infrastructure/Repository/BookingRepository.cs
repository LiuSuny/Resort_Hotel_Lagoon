using ResortLagoon.Application.Common.Interfaces;
using ResortLagoon.Domain.Entities;
using ResortLagoon.Infrastructure.Data;

namespace ResortLagoon.Infrastructure.Repository
{
    public class BookingRepository(ApplicationDbContext _db) : Repository<Booking>(_db), IBookingRepository
    {
        public void Update(Booking entity)
        {
           _db.Bookings.Update(entity);
        }
    }
}
