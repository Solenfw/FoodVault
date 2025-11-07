using System;
using System.Collections.Generic;

namespace FoodVault.Models.Entities;

public partial class RecipeTag
{
    public string Id { get; set; } = null!;

    public string RecipeId { get; set; } = null!;

    public string TagId { get; set; } = null!;

    public virtual Recipe Recipe { get; set; } = null!;

    public virtual Tag Tag { get; set; } = null!;
}
