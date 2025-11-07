using System;
using System.ComponentModel.DataAnnotations;

namespace FoodVault.Models.Entities;

/// <summary>
/// Thực thể đại diện cho cài đặt hệ thống
/// </summary>
public partial class SystemSetting
{
    /// <summary>
    /// Khóa cài đặt (khóa chính)
    /// </summary>
    [Key]
    [MaxLength(100)]
    public string Key { get; set; } = null!;

    /// <summary>
    /// Giá trị của cài đặt
    /// </summary>
    [Required]
    [MaxLength(1000)]
    public string Value { get; set; } = null!;

    /// <summary>
    /// Mô tả về cài đặt (tùy chọn)
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Thời điểm cập nhật lần cuối
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

