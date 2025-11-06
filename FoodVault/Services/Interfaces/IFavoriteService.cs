using FoodVault.Models.Entities;

namespace FoodVault.Services.Interfaces;

public interface IFavoriteService
{
    Task<Favorite> AddFavoriteAsync(string userId, string recipeId, CancellationToken cancellationToken = default);
    Task<bool> RemoveFavoriteAsync(string userId, string recipeId, CancellationToken cancellationToken = default);
    Task<bool> IsFavoriteAsync(string userId, string recipeId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Recipe>> GetUserFavoriteRecipesAsync(string userId, CancellationToken cancellationToken = default);
}















