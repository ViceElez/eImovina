namespace eImovina.Shared.DTOs.Auth;

public class LoggedUserDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public int? EmployeeId { get; set; }
    public List<string> Roles { get; set; } = new();
}