namespace LagoonStay.Application.Common.Dto
{
    // ViewModel for Line Chart
    public class LineChartDto
    {
        // List of series data for the line chart
        public List<ChartData> Series { get; set; }

        // Categories for the x-axis of the line chart
        public string[] Categories { get; set; }
    }

    // Class representing the data for each series in the line chart
    public class ChartData
    {
        // Name of the series (e.g., "New Bookings", "New Customers")
        public string Name { get; set; }
        // Data points for the series
        public int[] Data { get; set; }
    }
}
