using FoodVault.Models.Entities;

namespace FoodVault.Services.Interfaces;

public interface IRecipeService
{
    Task<Recipe> CreateRecipeAsync(Recipe recipe, CancellationToken cancellationToken = default);
    Task<Recipe?> GetRecipeByIdAsync(string recipeId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Recipe>> GetUserRecipesAsync(string userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Recipe>> SearchRecipesAsync(string query, IEnumerable<string>? tagIds = null, CancellationToken cancellationToken = default);
    Task<Recipe> UpdateRecipeAsync(Recipe recipe, CancellationToken cancellationToken = default);
    Task<bool> DeleteRecipeAsync(string recipeId, CancellationToken cancellationToken = default);

    // Tags
    Task AddTagAsync(string recipeId, string tagId, CancellationToken cancellationToken = default);
    Task RemoveTagAsync(string recipeTagId, CancellationToken cancellationToken = default);

    // Ingredients
    Task<RecipeIngredient> AddIngredientAsync(RecipeIngredient ingredient, CancellationToken cancellationToken = default);
    Task<bool> RemoveIngredientAsync(string recipeIngredientId, CancellationToken cancellationToken = default);

    // Pagination
    Task<(IReadOnlyList<Recipe> Items, bool HasMore, int NextPage)> GetRecipesPaginatedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}


