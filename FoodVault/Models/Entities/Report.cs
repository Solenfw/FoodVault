using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodVault.Models.Entities;

/// <summary>
/// Thực thể đại diện cho báo cáo vi phạm hoặc khiếu nại trong hệ thống
/// </summary>
public partial class Report
{
    /// <summary>
    /// Mã định danh duy nhất cho báo cáo
    /// </summary>
    [Key]
    [MaxLength(450)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Mã định danh của người dùng tạo báo cáo
    /// </summary>
    [Required]
    [MaxLength(450)]
    public string ReporterId { get; set; } = null!;

    /// <summary>
    /// Loại đối tượng bị báo cáo (Recipe/Comment/User)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string TargetType { get; set; } = null!;

    /// <summary>
    /// Mã định danh của đối tượng bị báo cáo
    /// </summary>
    [Required]
    [MaxLength(450)]
    public string TargetId { get; set; } = null!;

    /// <summary>
    /// Lý do báo cáo
    /// </summary>
    [MaxLength(500)]
    public string? Reason { get; set; }

    /// <summary>
    /// Trạng thái của báo cáo (Pending/Resolved/Dismissed)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// Thời điểm tạo báo cáo
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Thời điểm giải quyết báo cáo (null nếu chưa được giải quyết)
    /// </summary>
    public DateTime? ResolvedAt { get; set; }

    /// <summary>
    /// Mã định danh của người quản trị xử lý báo cáo (null nếu chưa được xử lý)
    /// </summary>
    [MaxLength(450)]
    public string? ResolvedBy { get; set; }

    /// <summary>
    /// Người dùng tạo báo cáo
    /// </summary>
    [ForeignKey(nameof(ReporterId))]
    public virtual User Reporter { get; set; } = null!;

    /// <summary>
    /// Người quản trị xử lý báo cáo (nếu có)
    /// </summary>
    [ForeignKey(nameof(ResolvedBy))]
    public virtual User? Resolver { get; set; }
}

