using eImovina.Shared.Models.Equipments;

namespace eImovina.Shared.DTOs.Equipments
{
    public class EquipmentDto
    {
        public int Id { get; set; }
        public string InventoryNumber { get; set; } = string.Empty;
        public string? SerialNumber { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string Category { get; set; } = string.Empty;
        public int StatusId { get; set; }
        public string Status { get; set; } = string.Empty;
        public int CurrentLocationId { get; set; }
        public string CurrentLocation { get; set; } = string.Empty;
        public decimal? Value { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public string? Note { get; set; }
        public int? AssignedToEmployeeId { get; set; }
        public string? AssignedTo { get; set; }
        public string? CoverImageUrl { get; set; }

    }
}
