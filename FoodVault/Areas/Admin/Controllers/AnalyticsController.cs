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
    /// Controller xử lý các yêu cầu liên quan đến Analytics của Admin
    /// </summary>
    [Area("Admin")]
    [Authorize(Roles = "Admin,Moderator")]
    public class AnalyticsController : Controller
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<AnalyticsController> _logger;

        /// <summary>
        /// Khởi tạo instance mới của AnalyticsController
        /// </summary>
        /// <param name="dashboardService">Service xử lý dữ liệu analytics</param>
        /// <param name="logger">Logger để ghi log</param>
        public AnalyticsController(IDashboardService dashboardService, ILogger<AnalyticsController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        /// <summary>
        /// Hiển thị trang Analytics chính của Admin
        /// </summary>
        /// <returns>View hiển thị analytics với dữ liệu thống kê</returns>
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Loading analytics data");
                var model = await _dashboardService.GetDashboardDataAsync();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading analytics data");
                
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
                
                ViewData["Error"] = "Có lỗi xảy ra khi tải dữ liệu analytics. Vui lòng thử lại sau.";
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
    }
}

