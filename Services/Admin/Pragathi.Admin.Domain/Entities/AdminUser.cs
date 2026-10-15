using Pragathi.Admin.Domain.Enums;

namespace Pragathi.Admin.Domain.Entities;

public class AdminUser
{
    public int Id { get; set; }
    public string LoginId { get; set; } = "";        // what they type to log in
    public string Name { get; set; } = "";           // display name
    public string PasswordHash { get; set; } = "";   // NEVER the real password
    public AdminRole Role { get; set; } = AdminRole.Viewer;
    public bool IsActive { get; set; } = true;       // switch a user off without deleting them
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}   