using System;
using System.Collections.Generic;

namespace FoodVault.Models.Entities;

public partial class Rating
{
    public string Id { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public string RecipeId { get; set; } = null!;

    public int Rating1 { get; set; }

    public string? Comment { get; set; }

    public DateTime? RatedAt { get; set; }

    public virtual Recipe Recipe { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
