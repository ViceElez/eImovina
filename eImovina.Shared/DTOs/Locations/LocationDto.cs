namespace eImovina.Shared.DTOs.Locations
{
    public class LocationDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int LocationTypeId { get; set; }
        public string LocationType { get; set; } = string.Empty;
        public string? Address { get; set; }
        public bool IsActive { get; set; }
    }
}
