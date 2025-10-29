using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodVault.Models
{
    public class Comment
    {
        // --- Primary Key ---
        public int Id { get; set; }

        // --- Comment Details ---
        [Required]
        [StringLength(1000, MinimumLength = 1)]
        public string Text { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Default to current time


        // --- Relationships ---

        // 1. Relationship to the User who wrote it (One-to-Many)
        // A Comment is written by one User. A User can write many Comments.
        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }


        // 2. Relationship to the Recipe it's on (One-to-Many)
        // A Comment belongs to one Recipe. A Recipe can have many Comments.
        [Required]
        public int RecipeId { get; set; }

        [ForeignKey("RecipeId")]
        public Recipe Recipe { get; set; }
    }
}