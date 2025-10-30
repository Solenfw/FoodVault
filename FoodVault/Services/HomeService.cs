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
			var topFavorites = await _dbContext.Favorites
				.GroupBy(f => f.RecipeId)
				.Select(g => new { RecipeId = g.Key!, Count = g.Count() })
				.OrderByDescending(x => x.Count)
				.Take(6)
				.Join(_dbContext.Recipes,
					g => g.RecipeId,
					r => r.Id,
					(g, r) => new RecipeItem
					{
						Id = r.Id,
						Title = r.Title,
						Description = r.Description,
						// ImageUrl placeholder: extend Recipe entity if available
						ImageUrl = null,
						PrepTimeMinutes = r.PrepTimeMinutes,
						CookTimeMinutes = r.CookTimeMinutes,
						Servings = r.Servings
					})
				.ToListAsync();

			// Recent recipes by CreatedAt
			var recentRecipes = await _dbContext.Recipes
				.OrderByDescending(r => r.CreatedAt)
				.Take(12)
				.Select(r => new RecipeItem
				{
					Id = r.Id,
					Title = r.Title,
					Description = r.Description,
					ImageUrl = null,
					PrepTimeMinutes = r.PrepTimeMinutes,
					CookTimeMinutes = r.CookTimeMinutes,
					Servings = r.Servings
				})
				.ToListAsync();

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

