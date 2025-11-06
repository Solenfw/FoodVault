using System.ComponentModel.DataAnnotations;

namespace FoodVault.Areas.Admin.ViewModels;

public class IngredientListViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? DefaultUnit { get; set; }
    public double? DefaultCalories { get; set; }
    public double? DefaultProtein { get; set; }
    public double? DefaultFat { get; set; }
    public double? DefaultCarbs { get; set; }
    public string? ImageUrl { get; set; }
    public int RecipeCount { get; set; }
    public int FridgeCount { get; set; }
}

public class IngredientDetailsViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? DefaultUnit { get; set; }
    public double? DefaultCalories { get; set; }
    public double? DefaultProtein { get; set; }
    public double? DefaultFat { get; set; }
    public double? DefaultCarbs { get; set; }
    public string? ImageUrl { get; set; }
    public int RecipeCount { get; set; }
    public int FridgeCount { get; set; }
}

public class CreateIngredientViewModel
{
    [Required(ErrorMessage = "Tên nguyên liệu là bắt buộc")]
    [StringLength(450, ErrorMessage = "Tên không được vượt quá 450 ký tự")]
    public string Name { get; set; } = string.Empty;

    [StringLength(50)]
    public string? DefaultUnit { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Calories phải >= 0")]
    public double? DefaultCalories { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Protein phải >= 0")]
    public double? DefaultProtein { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Fat phải >= 0")]
    public double? DefaultFat { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Carbs phải >= 0")]
    public double? DefaultCarbs { get; set; }

    public string? ImageUrl { get; set; }
}

public class EditIngredientViewModel
{
    [Required]
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên nguyên liệu là bắt buộc")]
    [StringLength(450, ErrorMessage = "Tên không được vượt quá 450 ký tự")]
    public string Name { get; set; } = string.Empty;

    [StringLength(50)]
    public string? DefaultUnit { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Calories phải >= 0")]
    public double? DefaultCalories { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Protein phải >= 0")]
    public double? DefaultProtein { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Fat phải >= 0")]
    public double? DefaultFat { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Carbs phải >= 0")]
    public double? DefaultCarbs { get; set; }

    public string? ImageUrl { get; set; }
}

