namespace eImovina.Shared.DTOs.Inventories;
public class InventoryItemFilterDto
{
    public string? SearchText { get; set; }
    public bool? IsFound { get; set; }
    public bool? IsDamaged { get; set; }
    public int? LocationId { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
