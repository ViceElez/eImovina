namespace eImovina.Shared.DTOs.Equipments;

public class TransferEquipmentDto
{
    public int EquipmentId { get; set; }
    public int NewEmployeeId { get; set; }
    public string? Note { get; set; }
}