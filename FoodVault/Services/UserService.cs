using FoodVault.Models.Data;
using FoodVault.Models.Entities;
using FoodVault.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FoodVault.Services;

public sealed class UserService : IUserService
{
    private readonly FoodVaultDbContext _dbContext;
    private readonly ILogger<UserService> _logger;

    public UserService(FoodVaultDbContext dbContext, ILogger<UserService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<User?> GetProfileAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get profile for user {UserId}", userId);
            throw;
        }
    }

    public async Task<User> UpdateProfileAsync(User profile, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == profile.Id, cancellationToken);
            if (existing == null)
            {
                throw new KeyNotFoundException($"User {profile.Id} not found");
            }

            _dbContext.Entry(existing).CurrentValues.SetValues(profile);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return existing;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update profile for user {UserId}", profile.Id);
            throw;
        }
    }

    public async Task<UserStats> GetUserStatsAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var numRecipes = await _dbContext.Recipes.CountAsync(r => r.UserId == userId, cancellationToken);
            var numFavorites = await _dbContext.Favorites.CountAsync(f => f.UserId == userId, cancellationToken);
            var numFridges = await _dbContext.Fridges.CountAsync(f => f.UserId == userId, cancellationToken);

            return new UserStats
            {
                NumRecipes = numRecipes,
                NumFavorites = numFavorites,
                NumFridges = numFridges
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get stats for user {UserId}", userId);
            throw;
        }
    }
}









