using eImovina.Shared.Models.Users;

namespace eImovina.Shared.Models.Equipments
{
    public class EquipmentAssignment
    {
        public int Id { get; set; }

        public int EquipmentId { get; set; }
        public Equipment Equipment { get; set; } = null!;

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        public int StatusId { get; set; }
        public AssignmentStatus Status { get; set; } = null!;

        public DateTime AssignedAt { get; set; }
        public DateTime? ReturnedAt { get; set; }

        public int AssignedByUserId { get; set; }
        public User AssignedByUser { get; set; } = null!;

        public int? ReturnedByUserId { get; set; }
        public User? ReturnedByUser { get; set; }

        public string? Note { get; set; }
    }
}
