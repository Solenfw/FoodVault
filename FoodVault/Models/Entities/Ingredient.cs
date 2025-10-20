using System;
using System.Collections.Generic;

namespace FoodVault.Models.Entities;

public partial class Ingredient
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? DefaultUnit { get; set; }

    public double? DefaultCalories { get; set; }

    public double? DefaultProtein { get; set; }

    public double? DefaultFat { get; set; }

    public double? DefaultCarbs { get; set; }

    public virtual ICollection<FridgeIngredient> FridgeIngredients { get; set; } = new List<FridgeIngredient>();

    public virtual ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
}
