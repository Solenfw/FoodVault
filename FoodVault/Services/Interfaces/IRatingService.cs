using FoodVault.Models.Entities;

namespace FoodVault.Services.Interfaces;

public interface IRatingService
{
    Task<Rating> AddRatingAsync(string userId, string recipeId, int rating, string? comment, CancellationToken cancellationToken = default);
    Task<Rating?> UpdateRatingAsync(string userId, string recipeId, int rating, string? comment, CancellationToken cancellationToken = default);
    Task<bool> DeleteRatingAsync(string ratingId, CancellationToken cancellationToken = default);
    Task<Rating?> GetUserRatingForRecipeAsync(string userId, string recipeId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Rating>> GetRatingsForRecipeAsync(string recipeId, CancellationToken cancellationToken = default);
}

