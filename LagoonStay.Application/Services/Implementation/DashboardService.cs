using LagoonStay.Application.Common.Dto;
using LagoonStay.Application.Common.Interfaces;
using LagoonStay.Application.Common.Utilities;
using LagoonStay.Application.Services.Implementation.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoonStay.Application.Services.Implementation
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        static int previousMonth = DateTime.Now.Month == 1 ? 12 : DateTime.Now.Month - 1;
        readonly DateTime previousMonthStartDate = new(DateTime.Now.Year, previousMonth, 1);
        readonly DateTime currentMonthStartDate = new(DateTime.Now.Year, DateTime.Now.Month, 1);

        public DashboardService(IUnitOfWork unitOfWork)
        {
           _unitOfWork = unitOfWork; 
        }
        public async Task<PieChartDto> GetBookingPieChartData()
        {
            var totalBookings = _unitOfWork.Booking.GetAll(u => u.BookingDate >= DateTime.Now.AddDays(-30) &&
           (u.Status != SD.StatusPending || u.Status == SD.StatusCancelled));

            var customerWithOneBooking = totalBookings.GroupBy(b => b.UserId).Where(x => x.Count() == 1).Select(x => x.Key).ToList();

            int bookingsByNewCustomer = customerWithOneBooking.Count();
            int bookingsByReturningCustomer = totalBookings.Count() - bookingsByNewCustomer;

            PieChartDto PieChartDto = new()
            {
                Labels = new string[] { "New Customer Bookings", "Returning Customer Bookings" },
                Series = new decimal[] { bookingsByNewCustomer, bookingsByReturningCustomer }
            };

            return PieChartDto;
        }

        public async Task<LineChartDto> GetMemberAndBookingLineChartData()
        {
            // Get all bookings and users for the last 30 days, group them by date, and select the count of new bookings and new customers for each date
            var bookingData = _unitOfWork.Booking.GetAll(u => u.BookingDate >= DateTime.Now.AddDays(-30) &&
            u.BookingDate.Date <= DateTime.Now)
                .GroupBy(b => b.BookingDate.Date)
                .Select(u => new {
                    DateTime = u.Key,
                    NewBookingCount = u.Count()
                });

            // Get all users for the last 30 days, group them by date, and select the count of new customers for each date
            var customerData = _unitOfWork.User.GetAll(u => u.CreatedAt >= DateTime.Now.AddDays(-30) &&
            u.CreatedAt.Date <= DateTime.Now)
                .GroupBy(b => b.CreatedAt.Date)
                .Select(u => new {
                    DateTime = u.Key,
                    NewCustomerCount = u.Count()
                });


            var leftJoin = bookingData.GroupJoin(customerData, booking => booking.DateTime, customer => customer.DateTime,
                (booking, customer) => new
                {
                    booking.DateTime,
                    booking.NewBookingCount,
                    NewCustomerCount = customer.Select(x => x.NewCustomerCount).FirstOrDefault()
                });


            var rightJoin = customerData.GroupJoin(bookingData, customer => customer.DateTime, booking => booking.DateTime,
                (customer, booking) => new
                {
                    customer.DateTime,
                    NewBookingCount = booking.Select(x => x.NewBookingCount).FirstOrDefault(),
                    customer.NewCustomerCount
                });

            var mergedData = leftJoin.Union(rightJoin).OrderBy(x => x.DateTime).ToList();

            var newBookingData = mergedData.Select(x => x.NewBookingCount).ToArray();
            var newCustomerData = mergedData.Select(x => x.NewCustomerCount).ToArray();
            var categories = mergedData.Select(x => x.DateTime.ToString("MM/dd/yyyy")).ToArray();

            List<ChartData> chartDataList = new()
            {
                new ChartData
                {
                    Name = "New Bookings",
                    Data = newBookingData
                },
                new ChartData
                {
                    Name = "New Members",
                    Data = newCustomerData
                },
            };

            LineChartDto LineChartDto = new()
            {
                Categories = categories,
                Series = chartDataList
            };



            return LineChartDto;
        }

        public async Task<RadialBarChartDto> GetRegisterUserChartData()
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
            var RadialBarChartDto = SD.GetRadialChartDataModel(totalUsers.Count(), countByCurrentMonth, countByPreviousMonth);
            return RadialBarChartDto;
        }

        public async Task<RadialBarChartDto> GetRevenueChartData()
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

            return SD.GetRadialChartDataModel(totalRevenue, countByCurrentMonth, countByPreviousMonth);
        }

        public async Task<RadialBarChartDto> GetTotalBookingRadialChartData()
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
            var RadialBarChartDto = SD.GetRadialChartDataModel(totalBookings.Count(), countByCurrentMonth, countByPreviousMonth);

            return RadialBarChartDto;
        }

       
    }
}
