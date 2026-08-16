using eImovina.Shared.Models.Users;
using eImovina.Shared.Models.Locations;


namespace eImovina.Shared.Models.Inventories
{
    public class Inventory
    {
        public int Id { get; set; }

        public int LocationId { get; set; }
        public Location Location { get; set; } = null!;

        public int ResponsibleEmployeeId { get; set; }
        public Employee ResponsibleEmployee { get; set; } = null!;

        public int StatusId { get; set; }
        public InventoryStatus Status { get; set; } = null!;

        public DateTime? OpenedAt { get; set; }
        public DateTime? ClosedAt { get; set; }

        public int CreatedByUserId { get; set; }
        public User CreatedByUser { get; set; } = null!;

        public int? ClosedByUserId { get; set; }
        public User? ClosedByUser { get; set; }
        public string? Note { get; set; }
        public ICollection<InventoryItem> Items { get; set; } = new List<InventoryItem>();
    }
}
