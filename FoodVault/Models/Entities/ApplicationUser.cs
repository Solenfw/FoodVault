using Microsoft.AspNetCore.Identity;

namespace FoodVault.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string? AvatarUrl { get; set; }
        public string? DietaryPreferences { get; set; }
        public string? DietaryRestrictions { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties (map tới các entity hiện tại)
        public virtual ICollection<Fridge>? Fridges { get; set; }
        public virtual ICollection<Favorite>? Favorites { get; set; }
        public virtual ICollection<Recipe>? Recipes { get; set; }
        public virtual ICollection<Rating>? Ratings { get; set; }
    }
}
