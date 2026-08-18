namespace eImovina.Shared.DTOs.Equipments;

public class AssignEquipmentDto
{
    public int EquipmentId { get; set; }
    public int EmployeeId { get; set; }
    public string? Note { get; set; }
}