using System.ComponentModel.DataAnnotations;

namespace FoodVault.Areas.Admin.ViewModels;

public class TagListViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int RecipeCount { get; set; }
}

public class TagDetailsViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int RecipeCount { get; set; }
}

public class CreateTagViewModel
{
    [Required(ErrorMessage = "Tên tag là bắt buộc")]
    [StringLength(450, ErrorMessage = "Tên không được vượt quá 450 ký tự")]
    public string Name { get; set; } = string.Empty;
}

public class EditTagViewModel
{
    [Required]
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên tag là bắt buộc")]
    [StringLength(450, ErrorMessage = "Tên không được vượt quá 450 ký tự")]
    public string Name { get; set; } = string.Empty;
}

