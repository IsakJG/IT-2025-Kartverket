namespace Kartverket.Web.Models.Entities;
public class Role
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = "";
    public string? RoleDesc { get; set; }
    public List<UserRole> UserRoles { get; set; } = new();
}
