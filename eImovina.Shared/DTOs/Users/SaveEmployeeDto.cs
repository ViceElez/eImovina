namespace eImovina.Shared.DTOs.Users;

public class SaveEmployeeDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public int LocationId { get; set; }
    public bool IsActive { get; set; } = true;
}