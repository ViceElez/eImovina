namespace eImovina.Shared.DTOs.Equipments;

public class EquipmentFilterDto
{
    public string? SearchText { get; set; }
    public int? CategoryId { get; set; }
    public int? StatusId { get; set; }
    public int? LocationId { get; set; }
    public int? EmployeeId { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}