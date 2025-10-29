using System.ComponentModel.DataAnnotations.Schema;

namespace FoodVault.Models
{
    public class Favorite
    {
        // --- Primary Key ---
        public int Id { get; set; }


        // --- Relationships (Composite Foreign Key) ---

        // 1. Link to the User
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }


        // 2. Link to the Recipe
        public int RecipeId { get; set; }

        [ForeignKey("RecipeId")]
        public Recipe Recipe { get; set; }
    }
}