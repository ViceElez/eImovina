namespace eImovina.Shared.DTOs.WriteOffRequests;

public class SaveWriteOffRequestDto
{
    public int EquipmentId { get; set; }
    public string Reason { get; set; } = string.Empty;
}