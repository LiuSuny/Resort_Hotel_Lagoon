using LagoonStay.Application.Common.Interfaces;
using LagoonStay.Domain.Entities;
using LagoonStay.Infrastructure.Data;

namespace LagoonStay.Infrastructure.Repository
{
    public class AmenityRepository(ApplicationDbContext _db) : Repository<Amenity>(_db), IAmentityRepository
    {
        public void Update(Amenity amenity)
        {
           _db.Amenities.Update(amenity);
        }
    }
}
