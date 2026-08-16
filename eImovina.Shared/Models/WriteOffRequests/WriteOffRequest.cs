using eImovina.Shared.Models.Users;
using eImovina.Shared.Models.Equipments;

namespace eImovina.Shared.Models.WriteOffRequests
{
    public class WriteOffRequest
    {
        public int Id { get; set; }
        public int EquipmentId { get; set; }
        public Equipment Equipment { get; set; } = null!;
        public int RequestedByUserId { get; set; }
        public User RequestedByUser { get; set; } = null!;
        public int StatusId { get; set; }
        public WriteOffRequestStatus Status { get; set; } = null!;
        public string Reason { get; set; } = null!;
        public DateTime RequestedAt { get; set; }
        public int? DecisionByUserId { get; set; }
        public User? DecisionByUser { get; set; }
        public string? DecisionNote { get; set; }
        public DateTime? DecidedAt { get; set; }
    }
}
