using FoodVault.Areas.Admin.ViewModels;

namespace FoodVault.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardViewModel> GetDashboardDataAsync();
    Task<List<ChartDataPoint>> GetUserGrowthDataAsync(int days = 30);
    Task<List<RecipeStats>> GetPopularRecipesAsync(int count = 10);
    Task<List<ActivityLog>> GetRecentActivitiesAsync(int count = 20);
    Task<SystemHealthModel> GetSystemHealthAsync();
}

