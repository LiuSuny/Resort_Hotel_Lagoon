using LagoonStay.Application.Common.Interfaces;
using LagoonStay.Domain.Entities;
using LagoonStay.Infrastructure.Data;

namespace LagoonStay.Infrastructure.Repository
{
    public class ApplicationUserRepository(ApplicationDbContext _db) 
        : Repository<ApplicationUser>(_db), IApplicationUserRepository
    {
       
    }
}
