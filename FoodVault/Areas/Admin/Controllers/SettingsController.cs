using System;
using FoodVault.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FoodVault.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SettingsController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SettingsController> _logger;

        public SettingsController(IConfiguration configuration, ILogger<SettingsController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public IActionResult Index()
        {
            try
            {
                var settings = new SettingsViewModel
                {
                    SiteName = _configuration["SiteSettings:SiteName"] ?? "FoodVault",
                    SiteDescription = _configuration["SiteSettings:SiteDescription"] ?? "",
                    MaintenanceMode = _configuration["SiteSettings:MaintenanceMode"] ?? "false",
                    MaxFileSize = _configuration["SiteSettings:MaxFileSize"] ?? "5MB",
                    AllowedFileTypes = _configuration["SiteSettings:AllowedFileTypes"] ?? "image/jpeg,image/png",
                    EmailEnabled = _configuration["EmailSettings:Enabled"] ?? "false",
                    EmailServer = _configuration["EmailSettings:Server"] ?? "",
                    NotificationEnabled = _configuration["NotificationSettings:Enabled"] ?? "false"
                };

                return View(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading settings");
                TempData["Error"] = "Có lỗi xảy ra khi tải cài đặt.";
                
                // Return default settings to prevent null reference errors
                var defaultSettings = new SettingsViewModel();
                return View(defaultSettings);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateSettings(string settingType, string value)
        {
            try
            {
                // In a real application, you would save these to database or configuration file
                // For now, just log the change
                _logger.LogInformation("Setting {SettingType} updated to {Value} by {Admin}", 
                    settingType, value, User.Identity?.Name);

                TempData["Success"] = $"Đã cập nhật cài đặt {settingType}.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating setting {SettingType}", settingType);
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật cài đặt.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

