using System;
using System.Collections.Generic;

namespace FoodVault.Areas.Admin.ViewModels;

/// <summary>
/// ViewModel cho trang Dashboard của Admin
/// </summary>
public class DashboardViewModel
{
    /// <summary>
    /// Tổng số người dùng trong hệ thống
    /// </summary>
    public int TotalUsers { get; set; }

    /// <summary>
    /// Tổng số công thức trong hệ thống
    /// </summary>
    public int TotalRecipes { get; set; }

    /// <summary>
    /// Tổng số bình luận trong hệ thống
    /// </summary>
    public int TotalComments { get; set; }

    /// <summary>
    /// Đánh giá trung bình của tất cả công thức
    /// </summary>
    public decimal AverageRating { get; set; }

    /// <summary>
    /// Số người dùng mới đăng ký hôm nay
    /// </summary>
    public int NewUsersToday { get; set; }

    /// <summary>
    /// Số công thức mới được tạo hôm nay
    /// </summary>
    public int NewRecipesToday { get; set; }

    /// <summary>
    /// Số báo cáo đang chờ xử lý
    /// </summary>
    public int PendingReports { get; set; }

    /// <summary>
    /// Dữ liệu biểu đồ tăng trưởng người dùng
    /// </summary>
    public List<ChartDataPoint> UserGrowthData { get; set; } = new List<ChartDataPoint>();

    /// <summary>
    /// Danh sách các công thức phổ biến
    /// </summary>
    public List<RecipeStats> PopularRecipes { get; set; } = new List<RecipeStats>();

    /// <summary>
    /// Danh sách các hoạt động gần đây
    /// </summary>
    public List<ActivityLog> RecentActivities { get; set; } = new List<ActivityLog>();

    /// <summary>
    /// Thông tin sức khỏe hệ thống
    /// </summary>
    public SystemHealthModel SystemHealth { get; set; } = new SystemHealthModel();

    /// <summary>
    /// Khởi tạo instance mới của DashboardViewModel
    /// </summary>
    public DashboardViewModel()
    {
        UserGrowthData = new List<ChartDataPoint>();
        PopularRecipes = new List<RecipeStats>();
        RecentActivities = new List<ActivityLog>();
        SystemHealth = new SystemHealthModel();
    }
}

/// <summary>
/// Điểm dữ liệu cho biểu đồ
/// </summary>
public class ChartDataPoint
{
    /// <summary>
    /// Nhãn hiển thị trên biểu đồ
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Giá trị tương ứng với nhãn
    /// </summary>
    public int Value { get; set; }

    /// <summary>
    /// Khởi tạo instance mới của ChartDataPoint
    /// </summary>
    public ChartDataPoint()
    {
    }

    /// <summary>
    /// Khởi tạo instance mới của ChartDataPoint với nhãn và giá trị
    /// </summary>
    /// <param name="label">Nhãn hiển thị</param>
    /// <param name="value">Giá trị</param>
    public ChartDataPoint(string label, int value)
    {
        Label = label;
        Value = value;
    }
}

/// <summary>
/// Thống kê về công thức
/// </summary>
public class RecipeStats
{
    /// <summary>
    /// Mã định danh của công thức
    /// </summary>
    public string RecipeId { get; set; } = string.Empty;

    /// <summary>
    /// Tiêu đề của công thức
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Số lượt yêu thích của công thức
    /// </summary>
    public int FavoriteCount { get; set; }

    /// <summary>
    /// Đánh giá trung bình của công thức
    /// </summary>
    public decimal AvgRating { get; set; }

    /// <summary>
    /// Khởi tạo instance mới của RecipeStats
    /// </summary>
    public RecipeStats()
    {
    }

    /// <summary>
    /// Khởi tạo instance mới của RecipeStats với đầy đủ thông tin
    /// </summary>
    /// <param name="recipeId">Mã định danh công thức</param>
    /// <param name="title">Tiêu đề công thức</param>
    /// <param name="favoriteCount">Số lượt yêu thích</param>
    /// <param name="avgRating">Đánh giá trung bình</param>
    public RecipeStats(string recipeId, string title, int favoriteCount, decimal avgRating)
    {
        RecipeId = recipeId;
        Title = title;
        FavoriteCount = favoriteCount;
        AvgRating = avgRating;
    }
}

/// <summary>
/// Nhật ký hoạt động của người dùng
/// </summary>
public class ActivityLog
{
    /// <summary>
    /// Thời điểm xảy ra hoạt động
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Tên người dùng thực hiện hoạt động
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Hành động được thực hiện
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Chi tiết về hoạt động
    /// </summary>
    public string Details { get; set; } = string.Empty;

    /// <summary>
    /// Khởi tạo instance mới của ActivityLog
    /// </summary>
    public ActivityLog()
    {
    }

    /// <summary>
    /// Khởi tạo instance mới của ActivityLog với đầy đủ thông tin
    /// </summary>
    /// <param name="timestamp">Thời điểm</param>
    /// <param name="username">Tên người dùng</param>
    /// <param name="action">Hành động</param>
    /// <param name="details">Chi tiết</param>
    public ActivityLog(DateTime timestamp, string username, string action, string details)
    {
        Timestamp = timestamp;
        Username = username;
        Action = action;
        Details = details;
    }
}

/// <summary>
/// Mô hình thông tin sức khỏe hệ thống
/// </summary>
public class SystemHealthModel
{
    /// <summary>
    /// Mức sử dụng CPU (%)
    /// </summary>
    public double CpuUsage { get; set; }

    /// <summary>
    /// Mức sử dụng bộ nhớ (%)
    /// </summary>
    public double MemoryUsage { get; set; }

    /// <summary>
    /// Mức sử dụng dung lượng đĩa (%)
    /// </summary>
    public double DiskUsage { get; set; }

    /// <summary>
    /// Khởi tạo instance mới của SystemHealthModel
    /// </summary>
    public SystemHealthModel()
    {
    }

    /// <summary>
    /// Khởi tạo instance mới của SystemHealthModel với các thông số
    /// </summary>
    /// <param name="cpuUsage">Mức sử dụng CPU</param>
    /// <param name="memoryUsage">Mức sử dụng bộ nhớ</param>
    /// <param name="diskUsage">Mức sử dụng dung lượng đĩa</param>
    public SystemHealthModel(double cpuUsage, double memoryUsage, double diskUsage)
    {
        CpuUsage = cpuUsage;
        MemoryUsage = memoryUsage;
        DiskUsage = diskUsage;
    }
}

public sealed class StatsCardViewModel
{
    public string Icon { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int Change { get; set; }
    public string ChangeLabel { get; set; } = string.Empty;
    public string Type { get; set; } = "primary";
}

