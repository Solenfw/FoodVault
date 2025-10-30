using FoodVault.Models.Entities;

namespace FoodVault.Services.Interfaces;

public interface ISearchService
{
    Task<IReadOnlyList<Recipe>> SearchRecipesAsync(string query, IEnumerable<string>? tagIds = null, CancellationToken cancellationToken = default);
}














