namespace eImovina.Shared.DTOs.Inventories
{
    public class InventoryItemDto
    {
        public int Id { get; set; }
        public int InventoryId { get; set; }
        public int EquipmentId { get; set; }
        public string Equipment { get; set; } = string.Empty;
        public string ExpectedInventoryNumber { get; set; } = string.Empty;
        public int ExpectedLocationId { get; set; }
        public string ExpectedLocation { get; set; } = string.Empty;
        public bool? IsFound { get; set; }
        public int? FoundLocationId { get; set; }
        public string? FoundLocation { get; set; }
        public bool? IsDamaged { get; set; }
        public string? DamageNote { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? ProcessedBy { get; set; }
    }
}
