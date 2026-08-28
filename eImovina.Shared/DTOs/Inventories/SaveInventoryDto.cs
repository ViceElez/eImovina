namespace eImovina.Shared.DTOs.Inventories;
public class SaveInventoryDto
{
    public int LocationId { get; set; }
    public int ResponsibleEmployeeId { get; set; }
    public string? Note { get; set; }
}
