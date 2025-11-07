using FoodVault.Models.Data;
using FoodVault.Models.Entities;
using FoodVault.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FoodVault.Services;

public sealed class RecipeService : IRecipeService
{
    private readonly FoodVaultDbContext _dbContext;
    private readonly ILogger<RecipeService> _logger;
    private readonly IMemoryCache _cache;
    private const string HomeCacheKey = "home:index:data";

    public RecipeService(FoodVaultDbContext dbContext, ILogger<RecipeService> logger, IMemoryCache cache)
    {
        _dbContext = dbContext;
        _logger = logger;
        _cache = cache;
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
            InvalidateHomeCache();
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
                    .ThenInclude(ri => ri.Ingredient)
                .Include(r => r.RecipeTags)
                .Include(r => r.Steps)
                .Include(r => r.User)
                .Include(r => r.Ratings)
                .Include(r => r.Favorites)
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
            var recipes = await _dbContext.Recipes
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt ?? DateTime.MinValue)
                .ThenByDescending(r => r.CreatedAt ?? DateTime.MinValue)
                .ToListAsync(cancellationToken);
            
            // Remove duplicates by ID (in case of any data issues)
            return recipes
                .GroupBy(r => r.Id)
                .Select(g => g.First())
                .ToList();
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

    public async Task<(IReadOnlyList<Recipe> Items, bool HasMore, int NextPage)> GetRecipesPaginatedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 12;
            var skip = (page - 1) * pageSize;

            var query = _dbContext.Recipes
                .OrderByDescending(r => r.CreatedAt)
                .AsNoTracking();

            var items = await query
                .Skip(skip)
                .Take(pageSize + 1)
                .ToListAsync(cancellationToken);

            var hasMore = items.Count > pageSize;
            if (hasMore) items.RemoveAt(items.Count - 1);
            var nextPage = hasMore ? page + 1 : page;

            return (items, hasMore, nextPage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to paginate recipes page={Page} size={Size}", page, pageSize);
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
            InvalidateHomeCache();
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

            // Delete all related data before deleting the recipe
            // Delete recipe ingredients
            var recipeIngredients = await _dbContext.RecipeIngredients
                .Where(ri => ri.RecipeId == recipeId)
                .ToListAsync(cancellationToken);
            _dbContext.RecipeIngredients.RemoveRange(recipeIngredients);

            // Delete recipe tags
            var recipeTags = await _dbContext.RecipeTags
                .Where(rt => rt.RecipeId == recipeId)
                .ToListAsync(cancellationToken);
            _dbContext.RecipeTags.RemoveRange(recipeTags);

            // Delete steps
            var steps = await _dbContext.Steps
                .Where(s => s.RecipeId == recipeId)
                .ToListAsync(cancellationToken);
            _dbContext.Steps.RemoveRange(steps);

            // Delete favorites for this recipe
            var recipeFavorites = await _dbContext.Favorites
                .Where(f => f.RecipeId == recipeId)
                .ToListAsync(cancellationToken);
            _dbContext.Favorites.RemoveRange(recipeFavorites);

            // Delete ratings for this recipe
            var recipeRatings = await _dbContext.Ratings
                .Where(r => r.RecipeId == recipeId)
                .ToListAsync(cancellationToken);
            _dbContext.Ratings.RemoveRange(recipeRatings);

            // Now delete the recipe
            _dbContext.Recipes.Remove(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
            InvalidateHomeCache();
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
            InvalidateHomeCache();
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
            InvalidateHomeCache();
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
            InvalidateHomeCache();
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
            InvalidateHomeCache();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove recipe ingredient {RecipeIngredientId}", recipeIngredientId);
            throw;
        }
    }

    private void InvalidateHomeCache()
    {
        _cache.Remove(HomeCacheKey);
    }
}