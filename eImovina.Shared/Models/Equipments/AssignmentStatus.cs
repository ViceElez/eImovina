namespace eImovina.Shared.Models.Equipments
{
    public class AssignmentStatus
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ICollection<EquipmentAssignment> Assignments { get; set; } = new List<EquipmentAssignment>();
    }
}
