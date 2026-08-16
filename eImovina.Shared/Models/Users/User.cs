using eImovina.Shared.Models.Equipments;
using eImovina.Shared.Models.Inventories;
using eImovina.Shared.Models.WriteOffRequests;

namespace eImovina.Shared.Models.Users
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public int? EmployeeId { get; set; }
        public Employee? Employee { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public ICollection<EquipmentAssignment> AssignmentsGiven { get; set; } = new List<EquipmentAssignment>();
        public ICollection<EquipmentAssignment> AssignmentsReturned { get; set; } = new List<EquipmentAssignment>();
        public ICollection<Inventory> InventoriesCreated { get; set; } = new List<Inventory>();
        public ICollection<Inventory> InventoriesClosed { get; set; } = new List<Inventory>();
        public ICollection<InventoryItem> InventoryItemsProcessed { get; set; } = new List<InventoryItem>();
        public ICollection<EquipmentRequest> EquipmentRequestsResolved { get; set; } = new List<EquipmentRequest>();
        public ICollection<WriteOffRequest> WriteOffRequestsSubmitted { get; set; } = new List<WriteOffRequest>();
        public ICollection<WriteOffRequest> WriteOffRequestsDecided { get; set; } = new List<WriteOffRequest>();
        public ICollection<EquipmentFile> FilesUploaded { get; set; } = new List<EquipmentFile>();
    }
}
