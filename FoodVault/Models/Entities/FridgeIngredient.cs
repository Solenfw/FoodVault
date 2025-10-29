using System;
using System.Collections.Generic;

namespace FoodVault.Models.Entities;

public partial class FridgeIngredient
{
    public string Id { get; set; } = null!;

    public string FridgeId { get; set; } = null!;

    public string IngredientId { get; set; } = null!;

    public double Quantity { get; set; }

    public string? Unit { get; set; }

    public DateOnly? ExpirationDate { get; set; }

    public virtual Fridge Fridge { get; set; } = null!;

    public virtual Ingredient Ingredient { get; set; } = null!;
}
