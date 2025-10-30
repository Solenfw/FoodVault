using FoodVault.Models.Data;
using FoodVault.Models.Entities;
using FoodVault.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FoodVault.Services;

public sealed class RecipeService : IRecipeService
{
    private readonly FoodVaultDbContext _dbContext;
    private readonly ILogger<RecipeService> _logger;

    public RecipeService(FoodVaultDbContext dbContext, ILogger<RecipeService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Recipe> CreateRecipeAsync(Recipe recipe, CancellationToken cancellationToken = default)
    {
        try
        {
            recipe.Id = string.IsNullOrWhiteSpace(recipe.Id) ? Guid.NewGuid().ToString() : recipe.Id;
            recipe.CreatedAt = DateTime.UtcNow;
            recipe.UpdatedAt = DateTime.UtcNow;

            await _dbContext.Recipes.AddAsync(recipe, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return recipe;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create recipe for user {UserId}", recipe.UserId);
            throw;
        }
    }

    public async Task<Recipe?> GetRecipeByIdAsync(string recipeId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.Recipes
                .Include(r => r.RecipeIngredients)
                .Include(r => r.RecipeTags)
                .Include(r => r.Steps)
                .FirstOrDefaultAsync(r => r.Id == recipeId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get recipe {RecipeId}", recipeId);
            throw;
        }
    }

    public async Task<IReadOnlyList<Recipe>> GetUserRecipesAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.Recipes
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.UpdatedAt)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get recipes for user {UserId}", userId);
            throw;
        }
    }

    public async Task<IReadOnlyList<Recipe>> SearchRecipesAsync(string query, IEnumerable<string>? tagIds = null, CancellationToken cancellationToken = default)
    {
        try
        {
            IQueryable<Recipe> q = _dbContext.Recipes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var qLower = query.ToLower();
                q = q.Where(r => r.Title.ToLower().Contains(qLower) || (r.Description != null && r.Description.ToLower().Contains(qLower)));
            }

            if (tagIds != null)
            {
                var tagSet = tagIds.Where(t => !string.IsNullOrWhiteSpace(t)).ToHashSet();
                if (tagSet.Count > 0)
                {
                    q = q.Where(r => _dbContext.RecipeTags.Any(rt => rt.RecipeId == r.Id && tagSet.Contains(rt.TagId)));
                }
            }

            return await q.OrderByDescending(r => r.UpdatedAt).ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search recipes for query {Query}", query);
            throw;
        }
    }

    public async Task<Recipe> UpdateRecipeAsync(Recipe recipe, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await _dbContext.Recipes.FirstOrDefaultAsync(r => r.Id == recipe.Id, cancellationToken);
            if (existing == null)
            {
                throw new KeyNotFoundException($"Recipe {recipe.Id} not found");
            }

            _dbContext.Entry(existing).CurrentValues.SetValues(recipe);
            existing.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return existing;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update recipe {RecipeId}", recipe.Id);
            throw;
        }
    }

    public async Task<bool> DeleteRecipeAsync(string recipeId, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _dbContext.Recipes.FirstOrDefaultAsync(r => r.Id == recipeId, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            _dbContext.Recipes.Remove(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete recipe {RecipeId}", recipeId);
            throw;
        }
    }

    public async Task AddTagAsync(string recipeId, string tagId, CancellationToken cancellationToken = default)
    {
        try
        {
            var exists = await _dbContext.RecipeTags.AnyAsync(rt => rt.RecipeId == recipeId && rt.TagId == tagId, cancellationToken);
            if (exists)
            {
                return;
            }
            var rt = new RecipeTag
            {
                Id = Guid.NewGuid().ToString(),
                RecipeId = recipeId,
                TagId = tagId
            };
            await _dbContext.RecipeTags.AddAsync(rt, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add tag {TagId} to recipe {RecipeId}", tagId, recipeId);
            throw;
        }
    }

    public async Task RemoveTagAsync(string recipeTagId, CancellationToken cancellationToken = default)
    {
        try
        {
            var rt = await _dbContext.RecipeTags.FirstOrDefaultAsync(t => t.Id == recipeTagId, cancellationToken);
            if (rt == null) return;
            _dbContext.RecipeTags.Remove(rt);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove recipe tag {RecipeTagId}", recipeTagId);
            throw;
        }
    }

    public async Task<RecipeIngredient> AddIngredientAsync(RecipeIngredient ingredient, CancellationToken cancellationToken = default)
    {
        try
        {
            ingredient.Id = string.IsNullOrWhiteSpace(ingredient.Id) ? Guid.NewGuid().ToString() : ingredient.Id;
            await _dbContext.RecipeIngredients.AddAsync(ingredient, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return ingredient;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add ingredient {IngredientId} to recipe {RecipeId}", ingredient.IngredientId, ingredient.RecipeId);
            throw;
        }
    }

    public async Task<bool> RemoveIngredientAsync(string recipeIngredientId, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await _dbContext.RecipeIngredients.FirstOrDefaultAsync(ri => ri.Id == recipeIngredientId, cancellationToken);
            if (existing == null) return false;
            _dbContext.RecipeIngredients.Remove(existing);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove recipe ingredient {RecipeIngredientId}", recipeIngredientId);
            throw;
        }
    }
}


