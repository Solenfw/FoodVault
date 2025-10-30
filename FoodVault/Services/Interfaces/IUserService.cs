using FoodVault.Models.Entities;

namespace FoodVault.Services.Interfaces;

public interface IUserService
{
    Task<User?> GetProfileAsync(string userId, CancellationToken cancellationToken = default);
    Task<User> UpdateProfileAsync(User profile, CancellationToken cancellationToken = default);

    Task<UserStats> GetUserStatsAsync(string userId, CancellationToken cancellationToken = default);
}

public sealed class UserStats
{
    public int NumRecipes { get; set; }
    public int NumFavorites { get; set; }
    public int NumFridges { get; set; }
}









