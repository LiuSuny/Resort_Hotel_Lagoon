using LagoonStay.Application.Common.Dto;
using LagoonStay.Application.Common.Interfaces;
using LagoonStay.Application.Common.Utilities;
using LagoonStay.Application.Services.Implementation.Interface;
using Microsoft.AspNetCore.Mvc;
using static SkiaSharp.HarfBuzz.SKShaper;

namespace LagoonStay.Web.Controllers
{
    public class DashboardController : Controller
    {

        private readonly IDashboardService _dashboardService;
       
        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetTotalBookingRadialChartData()
        {
           var result = await _dashboardService.GetTotalBookingRadialChartData();
            return Json(result);

        }
        public async Task<IActionResult> GetRegisterUserChartData()
        {            
            return Json(await _dashboardService.GetRegisterUserChartData());
        }
        public async Task<IActionResult> GetRevenueChartData()
        {
            return Json(await _dashboardService.GetRevenueChartData());

        }
        public async Task<IActionResult> GetBookingPieChartData()
        {
            return Json(await _dashboardService.GetBookingPieChartData());
        }

        public async Task<IActionResult> GetMemberAndBookingLineChartData()
        {
            return Json(await _dashboardService.GetMemberAndBookingLineChartData());
        }

    }
}
