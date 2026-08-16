using eImovina.Shared.Models.Users;
using eImovina.Shared.Models.Equipments;
using eImovina.Shared.Models.Inventories;


namespace eImovina.Shared.Models.Locations
{
    public class Location
    {
        public int Id { get; set; } 
        public string Name { get; set; } = null!;
        public int LocationTypeId { get; set; }
        public LocationType LocationType { get; set; } = null!;
        public string? Address { get; set; }
        public bool IsActive { get; set; } = true;
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
        public ICollection<Equipment> Equipment { get; set; } = new List<Equipment>();
        public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
    }
}
