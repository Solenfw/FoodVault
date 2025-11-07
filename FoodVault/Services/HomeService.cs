using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using FoodVault.Models.Data;
using FoodVault.Models.ViewModels;
using FoodVault.Services.Interfaces;

namespace FoodVault.Services
{
	public class HomeService : IHomeService
	{
		private readonly FoodVaultDbContext _dbContext;
		private readonly IMemoryCache _cache;

		public HomeService(FoodVaultDbContext dbContext, IMemoryCache cache)
		{
			_dbContext = dbContext;
			_cache = cache;
		}

		public async Task<HomeViewModel> GetHomeAsync()
		{
			const string cacheKey = "home:index:data";
			if (_cache.TryGetValue(cacheKey, out HomeViewModel? cached) && cached != null)
			{
				return cached;
			}

			// Popular tags by usage count in recipe_tags (join to tags to avoid null navigation)
			var popularTags = await _dbContext.RecipeTags
				.GroupBy(rt => rt.TagId)
				.Select(g => new { TagId = g.Key!, Count = g.Count() })
				.Join(_dbContext.Tags,
					g => g.TagId,
					t => t.Id,
					(g, t) => new TagItem { Id = t.Id, Name = t.Name, Count = g.Count })
				.OrderByDescending(t => t.Count)
				.ThenBy(t => t.Name)
				.Take(12)
				.ToListAsync();

			// Top favorites: recipes with most favorites
			var topFavoritesData = await _dbContext.Favorites
				.GroupBy(f => f.RecipeId)
				.Select(g => new { RecipeId = g.Key!, Count = g.Count() })
				.OrderByDescending(x => x.Count)
				.Take(6)
				.Join(_dbContext.Recipes,
					g => g.RecipeId,
					r => r.Id,
					(g, r) => new { Recipe = r, FavoriteCount = g.Count })
				.ToListAsync();

			var topFavoritesRecipeIds = topFavoritesData.Select(x => x.Recipe.Id).ToList();
			var topFavoritesRatings = await _dbContext.Ratings
				.Where(r => topFavoritesRecipeIds.Contains(r.RecipeId))
				.GroupBy(r => r.RecipeId)
				.Select(g => new { RecipeId = g.Key, AvgRating = g.Average(r => (double)r.Rating1) })
				.ToDictionaryAsync(x => x.RecipeId, x => x.AvgRating);

			var topFavorites = topFavoritesData.Select(rf => new RecipeItem
			{
				Id = rf.Recipe.Id,
				Title = rf.Recipe.Title,
				Description = rf.Recipe.Description,
				ImageUrl = rf.Recipe.ImageUrl,
				PrepTimeMinutes = rf.Recipe.PrepTimeMinutes,
				CookTimeMinutes = rf.Recipe.CookTimeMinutes,
				Servings = rf.Recipe.Servings,
				Rating = topFavoritesRatings.ContainsKey(rf.Recipe.Id) ? topFavoritesRatings[rf.Recipe.Id] : 0
			}).ToList();

			// Recent recipes by CreatedAt
			var recentRecipesData = await _dbContext.Recipes
				.OrderByDescending(r => r.CreatedAt)
				.Take(12)
				.ToListAsync();

			var recentRecipeIds = recentRecipesData.Select(r => r.Id).ToList();
			var recentRecipesRatings = await _dbContext.Ratings
				.Where(r => recentRecipeIds.Contains(r.RecipeId))
				.GroupBy(r => r.RecipeId)
				.Select(g => new { RecipeId = g.Key, AvgRating = g.Average(r => (double)r.Rating1) })
				.ToDictionaryAsync(x => x.RecipeId, x => x.AvgRating);

			var recentRecipes = recentRecipesData.Select(r => new RecipeItem
			{
				Id = r.Id,
				Title = r.Title,
				Description = r.Description,
				ImageUrl = r.ImageUrl,
				PrepTimeMinutes = r.PrepTimeMinutes,
				CookTimeMinutes = r.CookTimeMinutes,
				Servings = r.Servings,
				Rating = recentRecipesRatings.ContainsKey(r.Id) ? recentRecipesRatings[r.Id] : 0
			}).ToList();

			var vm = new HomeViewModel
			{
				PopularTags = popularTags,
				TopFavorites = topFavorites,
				RecentRecipes = recentRecipes
			};

			_cache.Set(cacheKey, vm, new MemoryCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
			});

			return vm;
		}
	}
}

