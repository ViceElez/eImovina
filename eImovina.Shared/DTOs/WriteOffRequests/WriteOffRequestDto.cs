namespace eImovina.Shared.DTOs.WriteOffRequests
{
    public class WriteOffRequestDto
    {
        public int Id { get; set; }
        public int EquipmentId { get; set; }
        public string Equipment { get; set; } = string.Empty;
        public string InventoryNumber { get; set; } = string.Empty;
        public string RequestedBy { get; set; } = string.Empty;
        public int StatusId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
        public string? DecisionBy { get; set; }
        public string? DecisionNote { get; set; }
        public DateTime? DecidedAt { get; set; }
    }
}
