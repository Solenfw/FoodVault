using FoodVault.Models.Entities;

namespace FoodVault.Services.Interfaces;

public interface IFridgeService
{
    Task<IReadOnlyList<Fridge>> GetUserFridgesAsync(string userId, CancellationToken cancellationToken = default);
    Task<Fridge> CreateFridgeAsync(Fridge fridge, CancellationToken cancellationToken = default);
    Task<bool> DeleteFridgeAsync(string fridgeId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FridgeIngredient>> GetFridgeIngredientsAsync(string fridgeId, CancellationToken cancellationToken = default);
    Task<FridgeIngredient> AddIngredientAsync(FridgeIngredient ingredient, CancellationToken cancellationToken = default);
    Task<bool> RemoveIngredientAsync(string fridgeIngredientId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FridgeIngredient>> GetExpiringSoonAsync(string fridgeId, int days = 3, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Recipe>> GetRecipeSuggestionsAsync(string fridgeId, int take = 6, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Ingredient>> GetAllIngredientsAsync(CancellationToken cancellationToken = default);
}


