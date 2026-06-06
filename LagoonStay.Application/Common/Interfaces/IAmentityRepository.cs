using LagoonStay.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoonStay.Application.Common.Interfaces
{
    public interface IAmentityRepository : IRepository<Amenity>
    {
        void Update(Amenity amenity);
    }
}
