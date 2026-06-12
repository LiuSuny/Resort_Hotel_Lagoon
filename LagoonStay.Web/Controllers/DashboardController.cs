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
            var radialBarChartVM = GetRadialChartDataModel(totalBookings.Count(), countByCurrentMonth, countByPreviousMonth);

            return Json(radialBarChartVM);

        }
        public async Task<IActionResult> GetRegisterUserChartData()
        {
            // Get all users
            var totalUsers = _unitOfWork.User.GetAll();

            // Count the number of users registered for the current month and the previous month
            var countByCurrentMonth = totalUsers.Count(u => u.CreatedAt >= currentMonthStartDate &&
            u.CreatedAt <= DateTime.Now);

            // Count the number of users registered for the previous month
            var countByPreviousMonth = totalUsers.Count(u => u.CreatedAt >= previousMonthStartDate &&
            u.CreatedAt <= currentMonthStartDate);

            // Calculate the increase/decrease ratio and prepare the data for the radial bar chart
            var radialBarChartVM = GetRadialChartDataModel(totalUsers.Count(), countByCurrentMonth, countByPreviousMonth);
            return Json(radialBarChartVM);

        }
        public async Task<IActionResult> GetRevenueChartData()
        {
            // Get all users
            var totalBookings = _unitOfWork.Booking.GetAll(u => u.Status != SD.StatusPending
           || u.Status == SD.StatusCancelled);

            var totalRevenue = Convert.ToInt32(totalBookings.Sum(u => u.TotalCost));

            // Count the number of bookings for the current month and the previous month
            var countByCurrentMonth = totalBookings.Where(u => u.BookingDate >= currentMonthStartDate &&
            u.BookingDate <= DateTime.Now).Sum(u => u.TotalCost);

            // Count the number of bookings for the previous month
            var countByPreviousMonth = totalBookings.Where(u => u.BookingDate >= previousMonthStartDate &&
            u.BookingDate <= currentMonthStartDate).Sum(u => u.TotalCost);

            return Json(GetRadialChartDataModel(totalRevenue, countByCurrentMonth, countByPreviousMonth));

        }

        /// <summary>
        /// avoid code duplication by creating a method that calculates the increase/decrease ratio and prepares the data for the radial bar chart based on the 
        /// total count, current month count, and previous month count
        /// </summary>
        /// <param name="totalCount"></param>
        /// <param name="currentMonthCount"></param>
        /// <param name="previousMonthCount"></param>
        /// <returns></returns>
        private static RadialBarChartVM GetRadialChartDataModel(int totalCount, double currentMonthCount, double previousMonthCount)
        {

            // Calculate the increase/decrease ratio and prepare the data for the radial bar chart
            RadialBarChartVM radialBarChartVM = new();

            // If there are no users in the previous month, we can consider the increase ratio as 100% if there are users in the current month, otherwise 0%
            int increaseDecreaseRatio = 100;

            /// If there are users in the previous month, calculate the increase/decrease ratio based on the counts of the current and previous months
            if (previousMonthCount != 0)
            {
                // Calculate the increase/decrease ratio as a percentage
                increaseDecreaseRatio = Convert.ToInt32((currentMonthCount - previousMonthCount) / previousMonthCount * 100);
            }

            // Set the properties of the RadialBarChartVM based on the calculated values
            radialBarChartVM.TotalCount = totalCount;
            radialBarChartVM.CountInCurrentMonth = (int)currentMonthCount;
            radialBarChartVM.HasRatioIncreased = currentMonthCount > previousMonthCount;
            radialBarChartVM.Series = new int[] { increaseDecreaseRatio };

            return radialBarChartVM;
        }
            
    }
}
