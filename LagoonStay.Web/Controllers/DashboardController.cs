using LagoonStay.Application.Common.Interfaces;
using LagoonStay.Application.Common.Utilities;
using LagoonStay.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace LagoonStay.Web.Controllers
{
    public class DashboardController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;
        static int previousMonth = DateTime.Now.Month == 1 ? 12 : DateTime.Now.Month - 1;
        readonly DateTime previousMonthStartDate = new(DateTime.Now.Year, previousMonth, 1);
        readonly DateTime currentMonthStartDate = new(DateTime.Now.Year, DateTime.Now.Month, 1);
        public DashboardController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetTotalBookingRadialChartData()
        {
            // Get all bookings that are not pending or cancelled
            var totalBookings = _unitOfWork.Booking.GetAll(u => u.Status != SD.StatusPending
            || u.Status == SD.StatusCancelled);

            // Count the number of bookings for the current month and the previous month
            var countByCurrentMonth = totalBookings.Count(u => u.BookingDate >= currentMonthStartDate &&
            u.BookingDate <= DateTime.Now);

            // Count the number of bookings for the previous month
            var countByPreviousMonth = totalBookings.Count(u => u.BookingDate >= previousMonthStartDate &&
            u.BookingDate <= currentMonthStartDate);

            // Calculate the increase/decrease ratio and prepare the data for the radial bar chart
            RadialBarChartVM radialBarChartVM = new();

            // If there are no bookings in the previous month, we can consider the increase ratio as 100% if there are bookings in the current month, otherwise 0%
            int increaseDecreaseRatio = 100;

            // If there are bookings in the previous month, calculate the increase/decrease ratio based on the counts of the current and previous months
            if (countByPreviousMonth != 0)
            {
                // Calculate the increase/decrease ratio as a percentage
                increaseDecreaseRatio = Convert.ToInt32((countByCurrentMonth - countByPreviousMonth) / countByPreviousMonth * 100);
            }

            // Set the properties of the RadialBarChartVM based on the calculated values
            radialBarChartVM.TotalCount = totalBookings.Count();
            radialBarChartVM.CountInCurrentMonth = countByCurrentMonth;
            radialBarChartVM.HasRatioIncreased = currentMonthStartDate > previousMonthStartDate;
            radialBarChartVM.Series = new int[] { increaseDecreaseRatio };

            return Json(radialBarChartVM);

        }
    }
}
