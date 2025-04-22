using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ResortLagoon.Application.Common.Interfaces;
using ResortLagoon.Domain.Entities;
using ResortLagoon.Web.ViewModels;

namespace ResortLagoon.Web.Controllers
{
    public class AccountController(IUnitOfWork _unitOfWork,
        UserManager<ApplicationUser> _userManager,
        RoleManager<IdentityRole> _roleManager,
        SignInManager<ApplicationUser> _signInManager) : Controller
    {
        public IActionResult Login(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            LoginVM loginVM = new()
            {
                RedirectUrl = returnUrl
            };

            return View(loginVM);
        }

        public IActionResult Register()
        {
            return View();
        }
    }
}
