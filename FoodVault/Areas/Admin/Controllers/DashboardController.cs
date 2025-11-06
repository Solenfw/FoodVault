using System;
using System.Linq;
using System.Threading.Tasks;
using FoodVault.Areas.Admin.ViewModels;
using FoodVault.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FoodVault.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller xử lý các yêu cầu liên quan đến Dashboard của Admin
    /// </summary>
    [Area("Admin")]
    [Authorize(Roles = "Admin,Moderator")]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<DashboardController> _logger;

        /// <summary>
        /// Khởi tạo instance mới của DashboardController
        /// </summary>
        /// <param name="dashboardService">Service xử lý dữ liệu dashboard</param>
        /// <param name="logger">Logger để ghi log</param>
        public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        /// <summary>
        /// Hiển thị trang Dashboard chính của Admin
        /// </summary>
        /// <returns>View hiển thị dashboard với dữ liệu thống kê</returns>
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Loading dashboard data");
                var model = await _dashboardService.GetDashboardDataAsync();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading dashboard data");
                
                // Trả về error view với model lỗi
                var errorModel = new DashboardViewModel
                {
                    TotalUsers = 0,
                    TotalRecipes = 0,
                    TotalComments = 0,
                    AverageRating = 0,
                    NewUsersToday = 0,
                    NewRecipesToday = 0,
                    PendingReports = 0
                };
                
                ViewData["Error"] = "Có lỗi xảy ra khi tải dữ liệu dashboard. Vui lòng thử lại sau.";
                return View(errorModel);
            }
        }

        /// <summary>
        /// Lấy dữ liệu tăng trưởng người dùng dạng JSON cho Chart.js
        /// </summary>
        /// <param name="days">Số ngày cần lấy dữ liệu (mặc định 30 ngày)</param>
        /// <returns>JSON object với format { labels: [], data: [] }</returns>
        [HttpGet]
        public async Task<JsonResult> GetUserGrowth(int days = 30)
        {
            try
            {
                _logger.LogDebug("Getting user growth data for {Days} days", days);
                
                var growthData = await _dashboardService.GetUserGrowthDataAsync(days);
                
                // Format dữ liệu cho Chart.js
                var result = new
                {
                    labels = growthData.Select(d => d.Label).ToArray(),
                    data = growthData.Select(d => d.Value).ToArray()
                };
                
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting user growth data");
                
                // Trả về dữ liệu rỗng nếu có lỗi
                return Json(new
                {
                    labels = new string[0],
                    data = new int[0]
                });
            }
        }

        /// <summary>
        /// Lấy thông tin sức khỏe hệ thống dạng JSON
        /// Được gọi qua AJAX mỗi 30 giây để cập nhật real-time
        /// </summary>
        /// <returns>JSON object chứa thông tin CPU, Memory, Disk usage</returns>
        [HttpGet]
        public async Task<JsonResult> GetSystemHealth()
        {
            try
            {
                _logger.LogDebug("Getting system health data");
                
                var systemHealth = await _dashboardService.GetSystemHealthAsync();
                
                // Trả về JSON với format chuẩn
                return Json(new
                {
                    cpuUsage = Math.Round(systemHealth.CpuUsage, 2),
                    memoryUsage = Math.Round(systemHealth.MemoryUsage, 2),
                    diskUsage = Math.Round(systemHealth.DiskUsage, 2),
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting system health data");
                
                // Trả về giá trị mặc định nếu có lỗi
                return Json(new
                {
                    cpuUsage = 0.0,
                    memoryUsage = 0.0,
                    diskUsage = 0.0,
                    timestamp = DateTime.UtcNow,
                    error = "Không thể lấy thông tin hệ thống"
                });
            }
        }

        /// <summary>
        /// Hiển thị trang test để kiểm tra quyền truy cập admin và role của người dùng
        /// </summary>
        /// <returns>View Test hiển thị thông tin user và role checks</returns>
        [HttpGet]
        public IActionResult Test()
        {
            _logger.LogInformation("Admin test page accessed by user: {User}", User.Identity?.Name);
            return View();
        }
    }
}

