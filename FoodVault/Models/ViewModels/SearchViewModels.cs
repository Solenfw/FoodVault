namespace FoodVault.Models.ViewModels;

public sealed class SearchViewModel
{
    public string? Query { get; set; }
}

public sealed class SearchResultsViewModel
{
    public string? Query { get; set; }
    public IReadOnlyList<RecipeListItemViewModel> Results { get; set; } = Array.Empty<RecipeListItemViewModel>();
}

public sealed class SearchBoxViewModel
{
    public string? Action { get; set; }
    public string? Controller { get; set; }
    public string? Placeholder { get; set; }
    public string? Query { get; set; }
}









