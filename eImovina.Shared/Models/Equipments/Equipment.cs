using eImovina.Shared.Models.Inventories;
using eImovina.Shared.Models.Locations;
using eImovina.Shared.Models.WriteOffRequests;


namespace eImovina.Shared.Models.Equipments
{
    public class Equipment
    {
        public int Id { get; set; }

        public string InventoryNumber { get; set; } = null!;
        public string? SerialNumber { get; set; }

        public string Name { get; set; } = null!;

        public int CategoryId { get; set; }
        public EquipmentCategory Category { get; set; } = null!;

        public int StatusId { get; set; }
        public EquipmentStatus Status { get; set; } = null!;

        public int CurrentLocationId { get; set; }
        public Location CurrentLocation { get; set; } = null!;

        public decimal? Value { get; set; }

        public DateTime? PurchaseDate { get; set; }

        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<EquipmentAssignment> Assignments { get; set; } = new List<EquipmentAssignment>();
        public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
        public ICollection<WriteOffRequest> WriteOffRequests { get; set; } = new List<WriteOffRequest>();
        public ICollection<EquipmentFile> Files { get; set; } = new List<EquipmentFile>();
    }
}
