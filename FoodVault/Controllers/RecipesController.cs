using System.Security.Claims;
using FoodVault.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodVault.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class RecipesController : ControllerBase
{
	private readonly IRecipeService _recipeService;

	public RecipesController(IRecipeService recipeService)
	{
		_recipeService = recipeService;
	}

	[HttpGet]
	[AllowAnonymous]
	public async Task<IActionResult> GetRecipesPaginated([FromQuery] int page = 1, [FromQuery] int pageSize = 12, CancellationToken ct = default)
	{
		var (items, hasMore, nextPage) = await _recipeService.GetRecipesPaginatedAsync(page, pageSize, ct);
		var currentUserId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
		var data = items.Select(r => new {
			id = r.Id,
			title = r.Title,
			description = r.Description,
			imageUrl = r.ImageUrl,
			prepTimeMinutes = r.PrepTimeMinutes,
			cookTimeMinutes = r.CookTimeMinutes,
			servings = r.Servings,
			userId = r.UserId,
			canDelete = !string.IsNullOrEmpty(currentUserId) && currentUserId == r.UserId
		});
		return Ok(new { data, hasMore, nextPage });
	}
}


