using System.ComponentModel.DataAnnotations;

namespace FoodVault.Models.ViewModels;

public sealed class ContactFormViewModel
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Phone]
    public string? Phone { get; set; }

    [Required]
    [StringLength(1000)]
    public string Message { get; set; } = string.Empty;
}

public sealed class CareerApplicationViewModel
{
    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Phone]
    public string? Phone { get; set; }

    [Required]
    [StringLength(100)]
    public string Position { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? CoverLetter { get; set; }
}


