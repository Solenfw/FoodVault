using System;
using System.Collections.Generic;

namespace FoodVault.Models.Entities;

public partial class Step
{
    public string Id { get; set; } = null!;

    public string RecipeId { get; set; } = null!;

    public int StepNumber { get; set; }

    public string Instruction { get; set; } = null!;

    public string? ImageUrl { get; set; }

    public virtual Recipe Recipe { get; set; } = null!;
}
