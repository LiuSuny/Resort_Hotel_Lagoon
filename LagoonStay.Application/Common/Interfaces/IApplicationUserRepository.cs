using LagoonStay.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoonStay.Application.Common.Interfaces
{
    // This interface defines the contract for a repository that manages ApplicationUser entities.
    public interface IApplicationUserRepository : IRepository<ApplicationUser>
    {
    }
}
