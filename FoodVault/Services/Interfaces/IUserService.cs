using FoodVault.Models.Entities;

namespace FoodVault.Services.Interfaces;

public interface IUserService
{
    Task<User?> GetProfileAsync(string userId, CancellationToken cancellationToken = default);
    Task<User> UpdateProfileAsync(User profile, CancellationToken cancellationToken = default);

    Task<UserStats> GetUserStatsAsync(string userId, CancellationToken cancellationToken = default);
}

public interface IUserPreferencesService
{
	Task<string> GetThemeAsync(string userId, CancellationToken cancellationToken = default);
	Task SetThemeAsync(string userId, string theme, CancellationToken cancellationToken = default);
}

public interface IThemeService
{
	Task<string> ResolveThemeAsync(System.Security.Claims.ClaimsPrincipal user, Microsoft.AspNetCore.Http.HttpRequest request, CancellationToken cancellationToken = default);
	Task SetUserThemeAsync(string userId, string theme, CancellationToken cancellationToken = default);
	void SetThemeCookie(Microsoft.AspNetCore.Http.HttpResponse response, string theme);
	string? GetThemeCookie(Microsoft.AspNetCore.Http.HttpRequest request);
}


public sealed class UserStats
{
    public int NumRecipes { get; set; }
    public int NumFavorites { get; set; }
    public int NumFridges { get; set; }
}









