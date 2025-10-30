﻿using System;
using System.Collections.Generic;

namespace FoodVault.Models.Entities;

public partial class Tag
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public virtual ICollection<RecipeTag> RecipeTags { get; set; } = new List<RecipeTag>();
}
