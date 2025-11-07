using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodVault.Models.Entities;

/// <summary>
/// Thực thể đại diện cho hoạt động của người dùng trong hệ thống
/// </summary>
public partial class UserActivity
{
    /// <summary>
    /// Mã định danh duy nhất cho hoạt động
    /// </summary>
    [Key]
    [MaxLength(450)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Mã định danh của người dùng thực hiện hoạt động
    /// </summary>
    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = null!;

    /// <summary>
    /// Hành động được thực hiện (ví dụ: Login, CreateRecipe, UpdateProfile)
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Action { get; set; } = null!;

    /// <summary>
    /// Địa chỉ IP của người dùng khi thực hiện hành động
    /// </summary>
    [MaxLength(50)]
    public string? IpAddress { get; set; }

    /// <summary>
    /// Thông tin trình duyệt/client của người dùng
    /// </summary>
    [MaxLength(500)]
    public string? UserAgent { get; set; }

    /// <summary>
    /// Thời điểm tạo hoạt động (mặc định: thời điểm hiện tại)
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Người dùng liên quan đến hoạt động
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;
}

