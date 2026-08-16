namespace eImovina.Shared.Models.Equipments
{
    public class EquipmentCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ICollection<Equipment> Equipment { get; set; } = new List<Equipment>();
        public ICollection<EquipmentRequest> EquipmentRequests { get; set; } = new List<EquipmentRequest>();
    }
}
