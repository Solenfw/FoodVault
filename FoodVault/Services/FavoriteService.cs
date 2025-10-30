using FoodVault.Models.Data;
using FoodVault.Models.Entities;
using FoodVault.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FoodVault.Services;

public sealed class FavoriteService : IFavoriteService
{
    private readonly FoodVaultDbContext _dbContext;
    private readonly ILogger<FavoriteService> _logger;

    public FavoriteService(FoodVaultDbContext dbContext, ILogger<FavoriteService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Favorite> AddFavoriteAsync(string userId, string recipeId, CancellationToken cancellationToken = default)
    {
        try
        {
            var exists = await _dbContext.Favorites.AnyAsync(f => f.UserId == userId && f.RecipeId == recipeId, cancellationToken);
            if (exists)
            {
                throw new InvalidOperationException("Recipe is already favorited by this user.");
            }

            var fav = new Favorite
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                RecipeId = recipeId,
                FavoritedAt = DateTime.UtcNow
            };

            await _dbContext.Favorites.AddAsync(fav, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return fav;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add favorite. user={UserId} recipe={RecipeId}", userId, recipeId);
            throw;
        }
    }

    public async Task<bool> RemoveFavoriteAsync(string userId, string recipeId, CancellationToken cancellationToken = default)
    {
        try
        {
            var fav = await _dbContext.Favorites.FirstOrDefaultAsync(f => f.UserId == userId && f.RecipeId == recipeId, cancellationToken);
            if (fav == null)
            {
                return false;
            }

            _dbContext.Favorites.Remove(fav);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove favorite. user={UserId} recipe={RecipeId}", userId, recipeId);
            throw;
        }
    }

    public async Task<bool> IsFavoriteAsync(string userId, string recipeId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.Favorites.AnyAsync(f => f.UserId == userId && f.RecipeId == recipeId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check favorite. user={UserId} recipe={RecipeId}", userId, recipeId);
            throw;
        }
    }

    public async Task<IReadOnlyList<Recipe>> GetUserFavoriteRecipesAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var recipeIds = await _dbContext.Favorites
                .Where(f => f.UserId == userId)
                .Select(f => f.RecipeId)
                .ToListAsync(cancellationToken);

            return await _dbContext.Recipes
                .Where(r => recipeIds.Contains(r.Id))
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user favorite recipes for {UserId}", userId);
            throw;
        }
    }
}










