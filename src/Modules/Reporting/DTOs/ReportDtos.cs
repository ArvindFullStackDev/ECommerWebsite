namespace Reporting.DTOs;

public class SalesReportDto
{
    public decimal TotalSales { get; set; }
    public int TotalOrders { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int TotalProductsSold { get; set; }
    public List<DailySalesDto> DailySales { get; set; } = new();
}

public class DailySalesDto
{
    public DateTime Date { get; set; }
    public decimal SalesAmount { get; set; }
    public int OrderCount { get; set; }
}

public class TopProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int TotalSold { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class DashboardStatsDto
{
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public int TotalProducts { get; set; }
    public int TotalUsers { get; set; }
    public int PendingOrders { get; set; }
    public int LowStockItems { get; set; }
    public decimal RevenueToday { get; set; }
    public int OrdersToday { get; set; }
    public List<DailySalesDto> RecentSales { get; set; } = new();
    public List<TopProductDto> TopProducts { get; set; } = new();
}
