namespace eImovina.Shared.DTOs.Inventories;
public class SaveInventoryItemDto
{
    public bool IsFound { get; set; }
    public int? FoundLocationId { get; set; }
    public bool IsDamaged { get; set; }
    public string? DamageNote { get; set; }
}
