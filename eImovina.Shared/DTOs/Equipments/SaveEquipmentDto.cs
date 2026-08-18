namespace eImovina.Shared.DTOs.Equipments;

public class SaveEquipmentDto
{
    public string InventoryNumber { get; set; } = string.Empty;
    public string? SerialNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public int StatusId { get; set; }
    public int CurrentLocationId { get; set; }
    public decimal? Value { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public string? Note { get; set; }
}