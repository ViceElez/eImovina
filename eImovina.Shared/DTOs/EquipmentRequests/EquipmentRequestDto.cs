namespace eImovina.Shared.DTOs.EquipmentRequests;

public class EquipmentRequestDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string Employee { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string Category { get; set; } = string.Empty;
    public int StatusId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolvedBy { get; set; }
    public string? ResolutionNote { get; set; }
}