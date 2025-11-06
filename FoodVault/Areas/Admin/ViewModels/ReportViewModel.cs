using System.ComponentModel.DataAnnotations;

namespace FoodVault.Areas.Admin.ViewModels;

public class ReportListViewModel
{
    public string Id { get; set; } = string.Empty;
    public string ReporterName { get; set; } = string.Empty;
    public string ReporterId { get; set; } = string.Empty;
    public string TargetType { get; set; } = string.Empty;
    public string TargetId { get; set; } = string.Empty;
    public string TargetName { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolvedBy { get; set; }
}

public class ReportDetailsViewModel
{
    public string Id { get; set; } = string.Empty;
    public string ReporterName { get; set; } = string.Empty;
    public string ReporterId { get; set; } = string.Empty;
    public string TargetType { get; set; } = string.Empty;
    public string TargetId { get; set; } = string.Empty;
    public string TargetName { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolvedBy { get; set; }
    public string? ResolvedByName { get; set; }
}

public class ResolveReportViewModel
{
    [Required]
    public string Id { get; set; } = string.Empty;

    [Required]
    public string Action { get; set; } = string.Empty; // "Resolve", "Dismiss"

    public string? Notes { get; set; }
}

