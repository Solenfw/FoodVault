using FoodVault.Models.Entities;
using Microsoft.AspNetCore.Identity;


namespace FoodVault.Models
{
    public class User : IdentityUser
    {
<<<<<<< HEAD
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProfilePictureUrl { get; set; }
=======
        public string? Name { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? DietaryPreferences { get; set; }
        public string? DietaryRestrictions { get; set; }
        public string? ActivityHistory { get; set; }
        public string Role { get; set; } = "user";

        // Navigation properties
        public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public virtual ICollection<Fridge> Fridges { get; set; } = new List<Fridge>();
        public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();
        public virtual ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
        public virtual Login? Login { get; set; }
>>>>>>> origin/develop
    }
}