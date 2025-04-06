using ResortLagoon.Application.Common.Interfaces;
using ResortLagoon.Domain.Entities;
using ResortLagoon.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResortLagoon.Infrastructure.Repository
{
    public class VillaNumberRepository(ApplicationDbContext _db) : Repository<VillaNumber>(_db), IVillaNumberRepository
    {
       
        public void Update(VillaNumber entity)
        {
           _db.villaNumbers.Update(entity);
        }
    }
}
