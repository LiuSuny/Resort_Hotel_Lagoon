using Microsoft.EntityFrameworkCore;
using LagoonStay.Application.Common.Interfaces;
using LagoonStay.Domain.Entities;
using LagoonStay.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LagoonStay.Infrastructure.Repository
{
    public class VillaRepository : Repository<Villa>, IVillaRepository
    {
        private readonly ApplicationDbContext _db;
        public VillaRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
       
       

        public void Update(Villa entity)
        {
            _db.Villas.Update(entity);
        }
    }
}
