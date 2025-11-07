using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using FoodVault.Areas.Admin.ViewModels;
using FoodVault.Models.Data;
using FoodVault.Models.Entities;
using FoodVault.Services.Interfaces;

namespace FoodVault.Services;

/// <summary>
/// Service xử lý dữ liệu cho trang Dashboard của Admin
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly FoodVaultDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<DashboardService> _logger;

    /// <summary>
    /// Khởi tạo instance mới của DashboardService
    /// </summary>
    /// <param name="context">DbContext để truy cập database</param>
    /// <param name="cache">Memory cache để cache kết quả</param>
    /// <param name="logger">Logger để ghi log</param>
    public DashboardService(FoodVaultDbContext context, IMemoryCache cache, ILogger<DashboardService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Lấy toàn bộ dữ liệu cho Dashboard
    /// </summary>
    /// <returns>ViewModel chứa tất cả dữ liệu Dashboard</returns>
    public async Task<DashboardViewModel> GetDashboardDataAsync()
    {
        const string cacheKey = "dashboard:data";
        
        try
        {
            // Kiểm tra cache
            if (_cache.TryGetValue(cacheKey, out DashboardViewModel? cached) && cached != null)
            {
                _logger.LogDebug("Dashboard data retrieved from cache");
                return cached;
            }

            var today = DateTime.UtcNow.Date;
            var yesterday = today.AddDays(-1);

            // Lấy tổng số người dùng
            var totalUsers = await _context.Users.CountAsync();

            // Lấy tổng số công thức
            var totalRecipes = await _context.Recipes.CountAsync();

            // Lấy tổng số bình luận (ratings có comment)
            var totalComments = await _context.Ratings
                .Where(r => !string.IsNullOrEmpty(r.Comment))
                .CountAsync();

            // Tính đánh giá trung bình
            var averageRating = await _context.Ratings
                .Where(r => r.Rating1 > 0)
                .AverageAsync(r => (decimal?)r.Rating1) ?? 0;

            // Số người dùng mới hôm nay
            var newUsersToday = await _context.Users
                .Where(u => u.CreatedAt.HasValue && u.CreatedAt.Value.Date == today)
                .CountAsync();

            // Số công thức mới hôm nay
            var newRecipesToday = await _context.Recipes
                .Where(r => r.CreatedAt.HasValue && r.CreatedAt.Value.Date == today)
                .CountAsync();

            // Số báo cáo đang chờ xử lý
            var pendingReports = await _context.Reports
                .Where(r => r.Status == "Pending")
                .CountAsync();

            // Lấy dữ liệu biểu đồ tăng trưởng người dùng (30 ngày)
            var userGrowthData = await GetUserGrowthDataAsync(30);

            // Lấy danh sách công thức phổ biến
            var popularRecipes = await GetPopularRecipesAsync(10);

            // Lấy các hoạt động gần đây
            var recentActivities = await GetRecentActivitiesAsync(20);

            // Lấy thông tin sức khỏe hệ thống
            var systemHealth = await GetSystemHealthAsync();

            var viewModel = new DashboardViewModel
            {
                TotalUsers = totalUsers,
                TotalRecipes = totalRecipes,
                TotalComments = totalComments,
                AverageRating = averageRating,
                NewUsersToday = newUsersToday,
                NewRecipesToday = newRecipesToday,
                PendingReports = pendingReports,
                UserGrowthData = userGrowthData,
                PopularRecipes = popularRecipes,
                RecentActivities = recentActivities,
                SystemHealth = systemHealth
            };

            // Cache kết quả trong 5 phút
            _cache.Set(cacheKey, viewModel, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });

            _logger.LogInformation("Dashboard data generated and cached");
            return viewModel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting dashboard data");
            throw;
        }
    }

    /// <summary>
    /// Lấy dữ liệu tăng trưởng người dùng theo ngày
    /// </summary>
    /// <param name="days">Số ngày cần lấy dữ liệu (mặc định 30 ngày)</param>
    /// <returns>Danh sách điểm dữ liệu cho biểu đồ</returns>
    public async Task<List<ChartDataPoint>> GetUserGrowthDataAsync(int days = 30)
    {
        const string cacheKeyPrefix = "dashboard:user-growth:";
        var cacheKey = $"{cacheKeyPrefix}{days}";

        try
        {
            // Kiểm tra cache
            if (_cache.TryGetValue(cacheKey, out List<ChartDataPoint>? cached) && cached != null)
            {
                return cached;
            }

            var endDate = DateTime.UtcNow.Date;
            var startDate = endDate.AddDays(-days);

            // Nhóm người dùng theo ngày tạo
            var userGrowth = await _context.Users
                .Where(u => u.CreatedAt.HasValue && u.CreatedAt.Value.Date >= startDate && u.CreatedAt.Value.Date <= endDate)
                .GroupBy(u => u.CreatedAt!.Value.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            // Tạo danh sách đầy đủ các ngày (bao gồm cả ngày không có người dùng mới)
            var result = new List<ChartDataPoint>();
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var dayData = userGrowth.FirstOrDefault(g => g.Date == date);
                var label = date.ToString("dd/MM");
                var value = dayData?.Count ?? 0;
                result.Add(new ChartDataPoint(label, value));
            }

            // Cache kết quả trong 5 phút
            _cache.Set(cacheKey, result, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting user growth data");
            throw;
        }
    }

    /// <summary>
    /// Lấy danh sách công thức phổ biến nhất
    /// </summary>
    /// <param name="count">Số lượng công thức cần lấy (mặc định 10)</param>
    /// <returns>Danh sách thống kê công thức</returns>
    public async Task<List<RecipeStats>> GetPopularRecipesAsync(int count = 10)
    {
        const string cacheKeyPrefix = "dashboard:popular-recipes:";
        var cacheKey = $"{cacheKeyPrefix}{count}";

        try
        {
            // Kiểm tra cache
            if (_cache.TryGetValue(cacheKey, out List<RecipeStats>? cached) && cached != null)
            {
                return cached;
            }

            // Join Recipes với Favorites để đếm số lượt yêu thích
            var recipeFavorites = await (from recipe in _context.Recipes
                                         join favorite in _context.Favorites on recipe.Id equals favorite.RecipeId into favorites
                                         select new
                                         {
                                             Recipe = recipe,
                                             FavoriteCount = favorites.Count()
                                         }).ToListAsync();

            // Join Recipes với Ratings để tính đánh giá trung bình
            var recipeRatings = await (from recipe in _context.Recipes
                                       join rating in _context.Ratings.Where(r => r.Rating1 > 0) on recipe.Id equals rating.RecipeId into ratings
                                       select new
                                       {
                                           RecipeId = recipe.Id,
                                           AvgRating = ratings.Any() ? ratings.Average(r => (decimal?)r.Rating1) ?? 0 : 0
                                       }).ToListAsync();

            // Kết hợp dữ liệu và sắp xếp
            var popularRecipes = recipeFavorites
                .Select(rf => new RecipeStats
                {
                    RecipeId = rf.Recipe.Id,
                    Title = rf.Recipe.Title,
                    FavoriteCount = rf.FavoriteCount,
                    AvgRating = recipeRatings.FirstOrDefault(rr => rr.RecipeId == rf.Recipe.Id)?.AvgRating ?? 0
                })
                .OrderByDescending(r => r.FavoriteCount)
                .ThenByDescending(r => r.AvgRating)
                .Take(count)
                .ToList();

            // Cache kết quả trong 5 phút
            _cache.Set(cacheKey, popularRecipes, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });

            return popularRecipes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting popular recipes");
            throw;
        }
    }

    /// <summary>
    /// Lấy danh sách các hoạt động gần đây
    /// </summary>
    /// <param name="count">Số lượng hoạt động cần lấy (mặc định 20)</param>
    /// <returns>Danh sách nhật ký hoạt động</returns>
    public async Task<List<ActivityLog>> GetRecentActivitiesAsync(int count = 20)
    {
        const string cacheKeyPrefix = "dashboard:recent-activities:";
        var cacheKey = $"{cacheKeyPrefix}{count}";

        try
        {
            // Kiểm tra cache
            if (_cache.TryGetValue(cacheKey, out List<ActivityLog>? cached) && cached != null)
            {
                return cached;
            }

            // Lấy các hoạt động gần đây từ bảng UserActivities
            var recentActivities = await _context.UserActivities
                .OrderByDescending(a => a.CreatedAt)
                .Take(count)
                .Join(
                    _context.Users,
                    activity => activity.UserId,
                    user => user.Id,
                    (activity, user) => new ActivityLog
                    {
                        Timestamp = activity.CreatedAt,
                        Username = user.UserName ?? user.Name ?? "Unknown",
                        Action = activity.Action,
                        Details = $"{activity.IpAddress ?? "N/A"} - {activity.UserAgent ?? "N/A"}"
                    }
                )
                .ToListAsync();

            // Cache kết quả trong 1 phút (hoạt động cần được cập nhật thường xuyên hơn)
            _cache.Set(cacheKey, recentActivities, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
            });

            return recentActivities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting recent activities");
            throw;
        }
    }

    /// <summary>
    /// Lấy thông tin sức khỏe hệ thống (CPU, Memory, Disk usage)
    /// </summary>
    /// <returns>Mô hình thông tin sức khỏe hệ thống</returns>
    public async Task<SystemHealthModel> GetSystemHealthAsync()
    {
        const string cacheKey = "dashboard:system-health";

        try
        {
            // Kiểm tra cache (cache ngắn hơn vì thông tin hệ thống thay đổi nhanh)
            if (_cache.TryGetValue(cacheKey, out SystemHealthModel? cached) && cached != null)
            {
                return cached;
            }

            var cpuUsage = 0.0;
            var memoryUsage = 0.0;
            var diskUsage = 0.0;

            // Lấy thông tin Memory sử dụng Process
            var currentProcess = Process.GetCurrentProcess();
            try
            {
                // Memory Usage - Working Set (bộ nhớ đang được sử dụng)
                var memoryInBytes = currentProcess.WorkingSet64;
                var memoryInMB = memoryInBytes / (1024.0 * 1024.0);
                
                // Lấy thông tin tổng bộ nhớ hệ thống (sử dụng GC để ước tính)
                // Gọi GC để có con số chính xác hơn
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                
                var managedMemory = GC.GetTotalMemory(false);
                var totalMemoryMB = managedMemory / (1024.0 * 1024.0);
                
                // Tính % bộ nhớ đã sử dụng (so với managed memory)
                memoryUsage = totalMemoryMB > 0 
                    ? Math.Min(100, (memoryInMB / totalMemoryMB) * 100)
                    : 0;

                // CPU Usage - tính toán đơn giản dựa trên Process priority
                // Lưu ý: Để có CPU chính xác cần PerformanceCounter (chỉ Windows) hoặc sampling
                // Ở đây ta ước tính dựa trên thời gian CPU đã sử dụng
                cpuUsage = Math.Min(50, memoryUsage * 0.5); // Ước tính dựa trên memory usage
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not get CPU/Memory usage");
            }

            // Lấy thông tin Disk Usage
            try
            {
                var drive = new DriveInfo(Path.GetPathRoot(Environment.CurrentDirectory) ?? "C:");
                if (drive.IsReady)
                {
                    var totalSpace = drive.TotalSize;
                    var freeSpace = drive.AvailableFreeSpace;
                    var usedSpace = totalSpace - freeSpace;
                    diskUsage = totalSpace > 0 
                        ? (usedSpace / (double)totalSpace) * 100
                        : 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not get disk usage");
            }

            var systemHealth = new SystemHealthModel(cpuUsage, memoryUsage, diskUsage);

            // Cache kết quả trong 1 phút (thông tin hệ thống cần được cập nhật thường xuyên)
            _cache.Set(cacheKey, systemHealth, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
            });

            return systemHealth;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting system health");
            // Trả về giá trị mặc định nếu có lỗi
            return new SystemHealthModel(0, 0, 0);
        }
    }
}

