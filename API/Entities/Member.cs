using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities;

public class Member
{
    public string Id { get; set; } = null!;
    public DateOnly DateOfBirth { get; set; }
    /*
        Although the ImageUrl and the DisplayName exist in AppUser,
        we still want thes properties here, for efficient fast-access,
        without joining the Member table with the AppUser table.
    */
    public string? ImageUrl { get; set; } 
    public required string DisplayName { get; set; }
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public DateTime LastActive { get; set; } = DateTime.UtcNow;
    public required string Gender { get; set; }
    public string? Description { get; set; }
    public required string City { get; set; }
    public required string Country { get; set; }

    // Navigation property

    public List<Photo> Photos { get; set; } = [];

    [ForeignKey(nameof(Id))]
    public AppUser User { get; set; } = null!;
}
