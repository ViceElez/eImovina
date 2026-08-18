namespace eImovina.Shared.DTOs.Equipments;

public class EquipmentAssignmentDto
{
    public int Id { get; set; }
    public int EquipmentId { get; set; }
    public string Equipment { get; set; } = string.Empty;
    public string InventoryNumber { get; set; } = string.Empty;
    public int EmployeeId { get; set; }
    public string Employee { get; set; } = string.Empty;
    public int StatusId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public string AssignedBy { get; set; } = string.Empty;
    public string? ReturnedBy { get; set; }
    public string? Note { get; set; }
}