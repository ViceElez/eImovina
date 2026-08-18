namespace eImovina.Shared.DTOs.Inventories
{
    public class InventoryDto
    {
        public int Id { get; set; }
        public int LocationId { get; set; }
        public string Location { get; set; } = string.Empty;
        public int ResponsibleEmployeeId { get; set; }
        public string ResponsibleEmployee { get; set; } = string.Empty;
        public int StatusId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? OpenedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public string? Note { get; set; }
        public int TotalItemsCount { get; set; }
        public int ProcessedItemsCount { get; set; }
        public int FoundCount { get; set; }
        public int MissingCount { get; set; }
        public int DamagedCount { get; set; }
    }
}
