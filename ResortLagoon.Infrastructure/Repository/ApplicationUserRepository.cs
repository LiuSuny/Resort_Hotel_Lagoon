using ResortLagoon.Application.Common.Interfaces;
using ResortLagoon.Domain.Entities;
using ResortLagoon.Infrastructure.Data;

namespace ResortLagoon.Infrastructure.Repository
{
    public class ApplicationUserRepository(ApplicationDbContext _db) 
        : Repository<ApplicationUser>(_db), IApplicationUserRepository
    {
       
    }
}
