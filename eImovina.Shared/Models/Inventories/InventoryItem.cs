using eImovina.Shared.Models.Equipments;
using eImovina.Shared.Models.Locations;
using eImovina.Shared.Models.Users;


namespace eImovina.Shared.Models.Inventories
{
    public class InventoryItem
    {
        public int Id { get; set; }
        public int InventoryId { get; set; }
        public Inventory Inventory { get; set; } = null!;
        public int EquipmentId { get; set; }
        public Equipment Equipment { get; set; } = null!;
        public int ExpectedLocationId { get; set; }
        public Location ExpectedLocation { get; set; } = null!;
        public string ExpectedInventoryNumber { get; set; } = null!;
        public bool? IsFound { get; set; }

        public int? FoundLocationId { get; set; }
        public Location? FoundLocation { get; set; }

        public bool? IsDamaged { get; set; }

        public string? DamageNote { get; set; }

        public DateTime? ProcessedAt { get; set; }

        public int? ProcessedByUserId { get; set; }
        public User? ProcessedByUser { get; set; }
    }
}
