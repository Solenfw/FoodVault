using FoodVault.Models.Data;
using FoodVault.Models.Entities;
using FoodVault.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FoodVault.Services;

public sealed class RatingService : IRatingService
{
    private readonly FoodVaultDbContext _dbContext;
    private readonly ILogger<RatingService> _logger;
    private readonly IMemoryCache _cache;
    private const string HomeCacheKey = "home:index:data";

    public RatingService(FoodVaultDbContext dbContext, ILogger<RatingService> logger, IMemoryCache cache)
    {
        _dbContext = dbContext;
        _logger = logger;
        _cache = cache;
    }

    private void InvalidateHomeCache()
    {
        _cache.Remove(HomeCacheKey);
    }

    public async Task<Rating> AddRatingAsync(string userId, string recipeId, int rating, string? comment, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await _dbContext.Ratings
                .FirstOrDefaultAsync(r => r.UserId == userId && r.RecipeId == recipeId, cancellationToken);

            if (existing != null)
            {
                existing.Rating1 = rating;
                existing.Comment = comment;
                existing.RatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync(cancellationToken);
                InvalidateHomeCache();
                return existing;
            }

            var ratingEntity = new Rating
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                RecipeId = recipeId,
                Rating1 = rating,
                Comment = comment,
                RatedAt = DateTime.UtcNow
            };

            await _dbContext.Ratings.AddAsync(ratingEntity, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            InvalidateHomeCache();
            return ratingEntity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add rating. user={UserId} recipe={RecipeId}", userId, recipeId);
            throw;
        }
    }

    public async Task<Rating?> UpdateRatingAsync(string userId, string recipeId, int rating, string? comment, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await _dbContext.Ratings
                .FirstOrDefaultAsync(r => r.UserId == userId && r.RecipeId == recipeId, cancellationToken);

            if (existing == null)
            {
                return null;
            }

            existing.Rating1 = rating;
            existing.Comment = comment;
            existing.RatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
            InvalidateHomeCache();
            return existing;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update rating. user={UserId} recipe={RecipeId}", userId, recipeId);
            throw;
        }
    }

    public async Task<bool> DeleteRatingAsync(string ratingId, CancellationToken cancellationToken = default)
    {
        try
        {
            var rating = await _dbContext.Ratings.FirstOrDefaultAsync(r => r.Id == ratingId, cancellationToken);
            if (rating == null)
            {
                return false;
            }

            _dbContext.Ratings.Remove(rating);
            await _dbContext.SaveChangesAsync(cancellationToken);
            InvalidateHomeCache();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete rating {RatingId}", ratingId);
            throw;
        }
    }

    public async Task<Rating?> GetUserRatingForRecipeAsync(string userId, string recipeId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.Ratings
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.UserId == userId && r.RecipeId == recipeId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user rating. user={UserId} recipe={RecipeId}", userId, recipeId);
            throw;
        }
    }

    public async Task<IReadOnlyList<Rating>> GetRatingsForRecipeAsync(string recipeId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.Ratings
                .Include(r => r.User)
                .Where(r => r.RecipeId == recipeId)
                .OrderByDescending(r => r.RatedAt)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get ratings for recipe {RecipeId}", recipeId);
            throw;
        }
    }
}

