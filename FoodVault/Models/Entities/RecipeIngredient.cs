using System;
using System.Collections.Generic;

namespace FoodVault.Models.Entities;

public partial class RecipeIngredient
{
    public string Id { get; set; } = null!;

    public string RecipeId { get; set; } = null!;

    public string IngredientId { get; set; } = null!;

    public double? Quantity { get; set; }

    public string? Unit { get; set; }

    public double? Calories { get; set; }

    public double? Protein { get; set; }

    public double? Fat { get; set; }

    public double? Carbs { get; set; }

    public virtual Ingredient Ingredient { get; set; } = null!;

    public virtual Recipe Recipe { get; set; } = null!;
}
