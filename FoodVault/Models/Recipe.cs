using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodVault.Models
{
    public class Recipe
    {
        // --- Primary Key ---
        // EF Core will automatically recognize this as the primary key for the Recipes table.
        public int Id { get; set; }

        // --- Basic Recipe Details ---
        [Required]
        [StringLength(100, ErrorMessage = "The title must be between 2 and 100 characters.", MinimumLength = 2)]
        public string Title { get; set; }

        [Required]
        [Display(Name = "List of Ingredients")]
        public string Ingredients { get; set; } // Stored as a single block of text.

        [Required]
        [Display(Name = "Cooking Instructions")]
        public string Instructions { get; set; } // Stored as a single block of text.

        [Required]
        [Display(Name = "Cooking Time (minutes)")]
        [Range(1, 1000, ErrorMessage = "Cooking time must be between 1 and 1000 minutes.")]
        public int CookingTimeInMinutes { get; set; }

        public DifficultyLevel Difficulty { get; set; }

        // This will store the web-accessible path to the uploaded image, e.g., "/uploads/recipes/my-image.jpg"
        public string? ImagePath { get; set; }


        // --- Relationships (The important part!) ---

        // 1. Relationship to the User (One-to-Many)
        // A Recipe is created by one User. A User can create many Recipes.

        // This is the Foreign Key that links this Recipe to a User.
        public string UserId { get; set; }

        // This is the "Navigation Property". It allows you to easily access the related User object.
        // For example: myRecipe.User.FirstName
        [ForeignKey("UserId")]
        public User User { get; set; }


        // 2. Relationship to Tags (Many-to-Many)
        // A Recipe can have many Tags. A Tag can be on many Recipes.
        public ICollection<Tag> Tags { get; set; } = new List<Tag>();


        // 3. Relationship to Comments (One-to-Many)
        // A Recipe can have many Comments. A Comment belongs to one Recipe.
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();


        // 4. Relationship to Favorites (One-to-Many, linking to the join table)
        // A Recipe can be favorited many times.
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    }
}