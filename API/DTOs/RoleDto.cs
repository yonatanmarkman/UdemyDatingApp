namespace API.DTOs;

public class RoleDto
{
    public string Id { get; set; } = "";
    public string? Email { get; set; } = "";
    public List<string>? Roles { get; set; } = null;
}
