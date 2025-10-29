using System;
using System.Collections.Generic;

namespace FoodVault.Models.Entities;

public partial class Favorite
{
    public string Id { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public string RecipeId { get; set; } = null!;

    public DateTime? FavoritedAt { get; set; }

    public virtual Recipe Recipe { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
