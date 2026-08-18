namespace eImovina.Shared.DTOs.Users
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int? EmployeeId { get; set; }
        public string? Employee { get; set; }
        public List<string> Roles { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }
}
