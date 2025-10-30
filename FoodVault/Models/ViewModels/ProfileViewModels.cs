using System.ComponentModel.DataAnnotations;

namespace FoodVault.Models.ViewModels;

public sealed class ProfileViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? DietaryPreferences { get; set; }
    public string? DietaryRestrictions { get; set; }
    public string? AvatarUrl { get; set; }

    public int NumRecipes { get; set; }
    public int NumFavorites { get; set; }
    public int NumFridges { get; set; }
}

public sealed class EditProfileViewModel
{
    [Required]
    public string Id { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? DietaryPreferences { get; set; }
    public string? DietaryRestrictions { get; set; }
}

public sealed class UpdateAvatarViewModel
{
    [Required]
    public string Id { get; set; } = string.Empty;
}

public sealed class UserProfileSidebarViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }

    public int NumRecipes { get; set; }
    public int NumFavorites { get; set; }
    public int NumFridges { get; set; }
}









