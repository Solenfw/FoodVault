using FoodVault.Models.Data;
using FoodVault.Models.Entities;
using FoodVault.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

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

public sealed class UserPreferencesService : IUserPreferencesService
{
	private readonly FoodVaultDbContext _db;
	private readonly ILogger<UserPreferencesService> _logger;

	public UserPreferencesService(FoodVaultDbContext db, ILogger<UserPreferencesService> logger)
	{
		_db = db;
		_logger = logger;
	}

	public async Task<string> GetThemeAsync(string userId, CancellationToken cancellationToken = default)
	{
		var pref = await _db.UserPreferences.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
		return pref?.Theme ?? "auto";
	}

	public async Task SetThemeAsync(string userId, string theme, CancellationToken cancellationToken = default)
	{
		if (theme != "light" && theme != "dark" && theme != "auto") theme = "auto";
		var pref = await _db.UserPreferences.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
		if (pref == null)
		{
			pref = new FoodVault.Models.Entities.UserPreferences { Id = Guid.NewGuid().ToString(), UserId = userId, Theme = theme, UpdatedAt = DateTime.UtcNow };
			await _db.UserPreferences.AddAsync(pref, cancellationToken);
		}
		else
		{
			pref.Theme = theme;
			pref.UpdatedAt = DateTime.UtcNow;
		}
		await _db.SaveChangesAsync(cancellationToken);
	}
}









