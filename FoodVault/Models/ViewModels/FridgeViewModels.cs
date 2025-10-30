using System.ComponentModel.DataAnnotations;

namespace FoodVault.Models.ViewModels;

public sealed class FridgeListItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public sealed class AddFridgeItemViewModel
{
    [Required]
    public string FridgeId { get; set; } = string.Empty;
    [Required]
    public string IngredientId { get; set; } = string.Empty;
    public double? Quantity { get; set; }
    public string? Unit { get; set; }
}

public sealed class RemoveFridgeItemViewModel
{
    [Required]
    public string FridgeIngredientId { get; set; } = string.Empty;
}

public sealed class FridgeIngredientViewModel
{
    public string Id { get; set; } = string.Empty;
    public string IngredientName { get; set; } = string.Empty;
    public double Quantity { get; set; }
    public string? Unit { get; set; }
    public DateOnly? ExpirationDate { get; set; }
    public double? Calories { get; set; }
    public double? Protein { get; set; }
    public double? Fat { get; set; }
    public double? Carbs { get; set; }
}

public sealed class FridgeWithIngredientsViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<FridgeIngredientViewModel> Ingredients { get; set; } = new();
}










