using System.ComponentModel.DataAnnotations;

namespace FoodVault.Models
{
    public class Tag
    {
        // --- Primary Key ---
        public int Id { get; set; }

        // --- Tag Details ---
        [Required]
        [StringLength(50)]
        public string Name { get; set; }


        // --- Relationships ---

        // Many-to-Many Relationship with Recipe
        // This navigation property allows a Tag to be associated with many Recipes.
        public ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
    }
}