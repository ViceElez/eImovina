using eImovina.Shared.Models.Equipments;
using eImovina.Shared.Models.Locations;
using eImovina.Shared.Models.Inventories;

namespace eImovina.Shared.Models.Users
{
    public class Employee
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? Email { get; set; }
        public int LocationId { get; set; }
        public Location Location { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public User? User { get; set; }
        public ICollection<EquipmentAssignment> Assignments { get; set; } = new List<EquipmentAssignment>();
        public ICollection<Inventory> ResponsibleForInventories { get; set; } = new List<Inventory>();
        public ICollection<EquipmentRequest> EquipmentRequests { get; set; } = new List<EquipmentRequest>();
    }
}
