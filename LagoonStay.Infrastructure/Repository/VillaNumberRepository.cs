using LagoonStay.Application.Common.Interfaces;
using LagoonStay.Domain.Entities;
using LagoonStay.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoonStay.Infrastructure.Repository
{
    public class VillaNumberRepository(ApplicationDbContext _db) : Repository<VillaNumber>(_db), IVillaNumberRepository
    {
       
        public void Update(VillaNumber entity)
        {
           _db.villaNumbers.Update(entity);
        }
    }
}
