using eImovina.Shared.DTOs.WriteOffRequests;

namespace eImovina.Shared.DTOs.Equipments;

public class EquipmentProfileDto
{
    public EquipmentDto Equipment { get; set; } = null!;
    public List<EquipmentAssignmentDto> AssignmentHistory { get; set; } = new();
    public List<EquipmentFileDto> Files { get; set; } = new();
    public List<WriteOffRequestDto> WriteOffRequests { get; set; } = new();
}