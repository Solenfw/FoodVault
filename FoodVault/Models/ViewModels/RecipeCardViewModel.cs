using System.Collections.Generic;

namespace FoodVault.Models.ViewModels
{
	public class RecipeCardViewModel
	{
		public string Id { get; set; } = string.Empty;
		public string Title { get; set; } = string.Empty;
		public string? Description { get; set; }
		public string? ImageUrl { get; set; }
		public int? PrepTimeMinutes { get; set; }
		public int? CookTimeMinutes { get; set; }
		public int? Servings { get; set; }
		public double Rating { get; set; }
		public bool IsFavorited { get; set; }
	}

	public class FavoriteButtonViewModel
	{
		public string RecipeId { get; set; } = string.Empty;
		private bool _isFavorite;
		public bool IsFavorite { get { return _isFavorite; } set { _isFavorite = value; } }
		public bool IsFavorited { get { return _isFavorite; } set { _isFavorite = value; } }
	}

	public class StarRatingViewModel
	{
		public double Rating { get; set; }
		public int MaxStars { get; set; } = 5;
	}

	public class PaginationViewModel
	{
		public int PageNumber { get; set; }
		public int TotalPages { get; set; }
		public string Action { get; set; } = string.Empty;
		public string Controller { get; set; } = string.Empty;
		public IDictionary<string, string> RouteValues { get; set; } = new Dictionary<string, string>();
	}
}



