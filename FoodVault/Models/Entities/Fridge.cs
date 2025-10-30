using System;
using System.Collections.Generic;

namespace FoodVault.Models.Entities;

public partial class Fridge
{
    public string Id { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<FridgeIngredient> FridgeIngredients { get; set; } = new List<FridgeIngredient>();

    public virtual User User { get; set; } = null!;
}

