using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FoodVault.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FoodVault.Services;

public sealed class ThemeService : IThemeService
{
	private const string ThemeCookieName = "fv_theme";
	private readonly IUserPreferencesService _userPrefs;
	private readonly ILogger<ThemeService> _logger;

	public ThemeService(IUserPreferencesService userPrefs, ILogger<ThemeService> logger)
	{
		_userPrefs = userPrefs;
		_logger = logger;
	}

	public async Task<string> ResolveThemeAsync(ClaimsPrincipal user, HttpRequest request, CancellationToken cancellationToken = default)
	{
		// Priority: cookie override -> user preference -> auto
		var cookieTheme = GetThemeCookie(request);
		if (IsValid(cookieTheme)) return cookieTheme!;

		if (user?.Identity?.IsAuthenticated == true)
		{
			var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
			if (!string.IsNullOrEmpty(userId))
			{
				var pref = await _userPrefs.GetThemeAsync(userId, cancellationToken);
				if (IsValid(pref)) return pref;
			}
		}

		return "auto";
	}

	public async Task SetUserThemeAsync(string userId, string theme, CancellationToken cancellationToken = default)
	{
		await _userPrefs.SetThemeAsync(userId, Normalize(theme), cancellationToken);
	}

	public void SetThemeCookie(HttpResponse response, string theme)
	{
		try
		{
			response.Cookies.Append(ThemeCookieName, Normalize(theme), new CookieOptions
			{
				HttpOnly = false,
				Secure = false,
				SameSite = SameSiteMode.Lax,
				Expires = DateTimeOffset.UtcNow.AddDays(30)
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to set theme cookie");
		}
	}

	public string? GetThemeCookie(HttpRequest request)
	{
		if (request.Cookies.TryGetValue(ThemeCookieName, out var v)) return v;
		return null;
	}

	private static bool IsValid(string? theme) => theme == "light" || theme == "dark" || theme == "auto";
	private static string Normalize(string? theme) => IsValid(theme) ? theme! : "auto";
}


