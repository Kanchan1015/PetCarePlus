using System.ComponentModel.DataAnnotations;

namespace PetCare.Domain.Inventory
{
    public class InventoryItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }
        
        [Required]
        public string Category { get; set; } = string.Empty;
        
        [Required]
        public string Supplier { get; set; } = string.Empty;

        public DateTime? ExpiryDate { get; set; }

        public string? Description { get; set; }

        public string? PhotoUrl { get; set; } // Optional photo file URL
    }
}