namespace eImovina.Shared.DTOs.Locations
{
    public class SaveLocationDto
    {
        public string Name { get; set; } = string.Empty;
        public int LocationTypeId { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
