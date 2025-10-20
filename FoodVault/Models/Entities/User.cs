using System;
using System.Collections.Generic;

namespace FoodVault.Models.Entities;

public partial class User
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? DietaryPreferences { get; set; }

    public string? DietaryRestrictions { get; set; }

    public string? ActivityHistory { get; set; }

    public string? Role { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? AvatarUrl { get; set; }

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual ICollection<Fridge> Fridges { get; set; } = new List<Fridge>();

    public virtual Login? Login { get; set; }

    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    public virtual ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
}
