using LagoonStay.Application.Common.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoonStay.Application.Services.Implementation.Interface
{
    public interface IDashboardService
    {
        Task<RadialBarChartDto> GetTotalBookingRadialChartData();
        Task<RadialBarChartDto> GetRegisterUserChartData();
        Task<RadialBarChartDto> GetRevenueChartData();
        Task<PieChartDto> GetBookingPieChartData();
        Task<LineChartDto> GetMemberAndBookingLineChartData();

    }
}
