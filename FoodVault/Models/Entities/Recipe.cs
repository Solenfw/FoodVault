using System;
using System.Collections.Generic;

namespace FoodVault.Models.Entities;

public partial class Recipe
{
    public string Id { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? Servings { get; set; }

    public int? PrepTimeMinutes { get; set; }

    public int? CookTimeMinutes { get; set; }

    public double? TotalCalories { get; set; }

    public double? TotalProtein { get; set; }

    public double? TotalFat { get; set; }

    public double? TotalCarbs { get; set; }

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    public virtual ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();

    public virtual ICollection<RecipeTag> RecipeTags { get; set; } = new List<RecipeTag>();

    public virtual ICollection<Step> Steps { get; set; } = new List<Step>();

    public virtual User User { get; set; } = null!;
}
