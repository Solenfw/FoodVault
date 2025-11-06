namespace FoodVault.Areas.Admin.ViewModels;

public class SettingsViewModel
{
    public string SiteName { get; set; } = "FoodVault";
    public string SiteDescription { get; set; } = "";
    public string MaintenanceMode { get; set; } = "false";
    public string MaxFileSize { get; set; } = "5MB";
    public string AllowedFileTypes { get; set; } = "image/jpeg,image/png";
    public string EmailEnabled { get; set; } = "false";
    public string EmailServer { get; set; } = "";
    public string NotificationEnabled { get; set; } = "false";
}

