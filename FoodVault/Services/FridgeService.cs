using FoodVault.Models.Data;
using FoodVault.Models.Entities;
using FoodVault.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FoodVault.Services;

public sealed class FridgeService : IFridgeService
{
    private readonly FoodVaultDbContext _dbContext;
    private readonly ILogger<FridgeService> _logger;

    public FridgeService(FoodVaultDbContext dbContext, ILogger<FridgeService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Fridge>> GetUserFridgesAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.Fridges.Where(f => f.UserId == userId).OrderBy(f => f.Name).ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get fridges for user {UserId}", userId);
            throw;
        }
    }

    public async Task<Fridge> CreateFridgeAsync(Fridge fridge, CancellationToken cancellationToken = default)
    {
        try
        {
            fridge.Id = string.IsNullOrWhiteSpace(fridge.Id) ? Guid.NewGuid().ToString() : fridge.Id;
            fridge.CreatedAt = DateTime.UtcNow;
            await _dbContext.Fridges.AddAsync(fridge, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return fridge;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create fridge for user {UserId}", fridge.UserId);
            throw;
        }
    }

    public async Task<bool> DeleteFridgeAsync(string fridgeId, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await _dbContext.Fridges.FirstOrDefaultAsync(f => f.Id == fridgeId, cancellationToken);
            if (existing == null)
            {
                return false;
            }
            _dbContext.Fridges.Remove(existing);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete fridge {FridgeId}", fridgeId);
            throw;
        }
    }

    public async Task<IReadOnlyList<FridgeIngredient>> GetFridgeIngredientsAsync(string fridgeId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.FridgeIngredients.Where(fi => fi.FridgeId == fridgeId).ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get ingredients for fridge {FridgeId}", fridgeId);
            throw;
        }
    }

    public async Task<FridgeIngredient> AddIngredientAsync(FridgeIngredient ingredient, CancellationToken cancellationToken = default)
    {
        try
        {
            ingredient.Id = string.IsNullOrWhiteSpace(ingredient.Id) ? Guid.NewGuid().ToString() : ingredient.Id;
            await _dbContext.FridgeIngredients.AddAsync(ingredient, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return ingredient;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add ingredient to fridge {FridgeId}", ingredient.FridgeId);
            throw;
        }
    }

    public async Task<bool> RemoveIngredientAsync(string fridgeIngredientId, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await _dbContext.FridgeIngredients.FirstOrDefaultAsync(fi => fi.Id == fridgeIngredientId, cancellationToken);
            if (existing == null)
            {
                return false;
            }
            _dbContext.FridgeIngredients.Remove(existing);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove ingredient {FridgeIngredientId}", fridgeIngredientId);
            throw;
        }
    }

    public async Task<IReadOnlyList<FridgeIngredient>> GetExpiringSoonAsync(string fridgeId, int days = 3, CancellationToken cancellationToken = default)
    {
        try
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var threshold = today.AddDays(days);
            return await _dbContext.FridgeIngredients
                .Where(fi => fi.FridgeId == fridgeId && fi.ExpirationDate != null && fi.ExpirationDate <= threshold)
                .OrderBy(fi => fi.ExpirationDate)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get expiring soon ingredients for fridge {FridgeId}", fridgeId);
            throw;
        }
    }

    public async Task<IReadOnlyList<Recipe>> GetRecipeSuggestionsAsync(string fridgeId, int take = 6, CancellationToken cancellationToken = default)
    {
        try
        {
            var ingredientIds = await _dbContext.FridgeIngredients
                .Where(fi => fi.FridgeId == fridgeId)
                .Select(fi => fi.IngredientId)
                .Distinct()
                .ToListAsync(cancellationToken);

            var query = _dbContext.Recipes
                .Where(r => _dbContext.RecipeIngredients.Any(ri => ri.RecipeId == r.Id && ingredientIds.Contains(ri.IngredientId)))
                .OrderByDescending(r => r.UpdatedAt)
                .Take(take);

            return await query.ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get recipe suggestions for fridge {FridgeId}", fridgeId);
            throw;
        }
    }

    public async Task<IReadOnlyList<Ingredient>> GetAllIngredientsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Ingredients.OrderBy(i => i.Name).ToListAsync(cancellationToken);
    }
}


