using System.ComponentModel.DataAnnotations;

namespace FoodVault.Models.ViewModels;

public sealed class RecipeListItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UserId { get; set; } = string.Empty;
}

public sealed class RecipeIngredientDetailsViewModel {
    public string IngredientName { get; set; } = string.Empty;
    public double? Quantity { get; set; }
    public string? Unit { get; set; }
    public double? Calories { get; set; }
    public double? Protein { get; set; }
    public double? Fat { get; set; }
    public double? Carbs { get; set; }
}
public sealed class StepViewModel {
    public int StepNumber { get; set; }
    public string Instruction { get; set; } = string.Empty;
}
public sealed class RecipeDetailsViewModel {
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int? Servings { get; set; }
    public int? PrepTimeMinutes { get; set; }
    public int? CookTimeMinutes { get; set; }
    public string? AuthorName { get; set; }
    public double? TotalCalories { get; set; }
    public double? TotalProtein { get; set; }
    public double? TotalFat { get; set; }
    public double? TotalCarbs { get; set; }
    public List<RecipeIngredientDetailsViewModel> Ingredients { get; set; } = new();
    public List<StepViewModel> Steps { get; set; } = new();
    public double? AvgRating { get; set; }
    public int? FavoriteCount { get; set; }
    public bool IsFavorited { get; set; }
    public List<RatingViewModel> Ratings { get; set; } = new();
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public sealed class RatingViewModel
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string RecipeId { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime? RatedAt { get; set; }
}

public sealed class AddRatingViewModel
{
    [Required]
    public string RecipeId { get; set; } = string.Empty;
    
    [Required]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public int Rating { get; set; }
    
    public string? Comment { get; set; }
}

public sealed class CreateRecipeViewModel
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? Servings { get; set; }
    public int? PrepTimeMinutes { get; set; }
    public int? CookTimeMinutes { get; set; }
}

public sealed class EditRecipeViewModel
{
    [Required]
    public string Id { get; set; } = string.Empty;
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? Servings { get; set; }
    public int? PrepTimeMinutes { get; set; }
    public int? CookTimeMinutes { get; set; }
    public string? ImageUrl { get; set; }
}

public sealed class AddRecipeIngredientViewModel
{
    [Required]
    public string RecipeId { get; set; } = string.Empty;
    [Required]
    public string IngredientId { get; set; } = string.Empty;
    public double? Quantity { get; set; }
    public string? Unit { get; set; }
}

public sealed class RemoveRecipeIngredientViewModel
{
    [Required]
    public string RecipeIngredientId { get; set; } = string.Empty;
}

public sealed class AddRecipeTagViewModel
{
    [Required]
    public string RecipeId { get; set; } = string.Empty;
    [Required]
    public string TagId { get; set; } = string.Empty;
}

public sealed class RemoveRecipeTagViewModel
{
    [Required]
    public string RecipeTagId { get; set; } = string.Empty;
}

public sealed class RecentRecipesViewModel
{
    public IReadOnlyList<RecipeListItemViewModel> Recipes { get; set; } = Array.Empty<RecipeListItemViewModel>();
}



