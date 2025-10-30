using System.Collections.Generic;

namespace FoodVault.Models.ViewModels
{
	public class HomeViewModel
	{
		public IReadOnlyList<TagItem> PopularTags { get; set; } = new List<TagItem>();
		public IReadOnlyList<RecipeItem> TopFavorites { get; set; } = new List<RecipeItem>();
		public IReadOnlyList<RecipeItem> RecentRecipes { get; set; } = new List<RecipeItem>();
	}

	public class TagItem
	{
		public string Id { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public int Count { get; set; }
	}

	public class RecipeItem
	{
		public string Id { get; set; } = string.Empty;
		public string Title { get; set; } = string.Empty;
		public string? Description { get; set; }
		public string? ImageUrl { get; set; }
		public int? PrepTimeMinutes { get; set; }
		public int? CookTimeMinutes { get; set; }
		public int? Servings { get; set; }
	}
}



