using FoodVault.Models.Data;
using FoodVault.Models.Entities;
using FoodVault.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FoodVault.Services;

public sealed class SearchService : ISearchService
{
    private readonly FoodVaultDbContext _dbContext;
    private readonly ILogger<SearchService> _logger;

    public SearchService(FoodVaultDbContext dbContext, ILogger<SearchService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
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
            _logger.LogError(ex, "Search failed for query {Query}", query);
            throw;
        }
    }
}











