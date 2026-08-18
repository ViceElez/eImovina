namespace eImovina.Shared.DTOs.Users;

public class SaveUserDto
{
    public string Username { get; set; } = string.Empty;
    public string? Password { get; set; }
    public int? EmployeeId { get; set; }
    public List<int> RoleIds { get; set; } = new();
    public bool IsActive { get; set; } = true;
}