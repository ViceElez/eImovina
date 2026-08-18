namespace eImovina.Shared.DTOs;

public class DashboardDto
{
    public int TotalEquipmentCount { get; set; }
    public int AssignedEquipmentCount { get; set; }
    public int InServiceEquipmentCount { get; set; }
    public int MissingEquipmentCount { get; set; }
    public int OpenEquipmentRequestsCount { get; set; }
    public int InventoriesInProgressCount { get; set; }
    public decimal? TotalEquipmentValue { get; set; }
    public List<RecentChangeDto> RecentChanges { get; set; } = new();
    public int? MyActiveAssignmentsCount { get; set; }
    public int? MyOpenRequestsCount { get; set; }
}

public class RecentChangeDto
{
    public DateTime OccurredAt { get; set; }
    public string Description { get; set; } = string.Empty;
}