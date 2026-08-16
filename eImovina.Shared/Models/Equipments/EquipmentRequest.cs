using eImovina.Shared.Models.Users;

namespace eImovina.Shared.Models.Equipments
{
    public class EquipmentRequest
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;
        public int CategoryId { get; set; }
        public EquipmentCategory Category { get; set; } = null!;
        public int StatusId { get; set; }
        public RequestStatus Status { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime RequestedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public int? ResolvedByUserId { get; set; }
        public User? ResolvedByUser { get; set; }
        public string? ResolutionNote { get; set; }
    }
}
