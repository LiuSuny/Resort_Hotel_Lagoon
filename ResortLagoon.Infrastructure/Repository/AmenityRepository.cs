using ResortLagoon.Application.Common.Interfaces;
using ResortLagoon.Domain.Entities;
using ResortLagoon.Infrastructure.Data;

namespace ResortLagoon.Infrastructure.Repository
{
    public class AmenityRepository(ApplicationDbContext _db) : Repository<Amenity>(_db), IAmentityRepository
    {
        public void Update(Amenity amenity)
        {
           _db.Amenities.Update(amenity);
        }
    }
}
