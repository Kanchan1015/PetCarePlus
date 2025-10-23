using System;

namespace PetCare.Application.Inventory
{
    public class InventoryItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Supplier { get; set; } = string.Empty;

        public DateTime? ExpiryDate { get; set; }
        public string? Description { get; set; }
        public string? PhotoUrl { get; set; } // Optional photo file URL
    }
}