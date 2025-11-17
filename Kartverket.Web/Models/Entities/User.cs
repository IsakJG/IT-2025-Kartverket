
namespace Kartverket.Web.Models.Entities;
public class User
{
    public int UserId { get; set; }
    public int OrgId { get; set; }
    public Organization? Organization { get; set; }
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public List<Report> Reports { get; set; } = new();
    public List<Report> AssignedReports { get; set; } = new();
    public List<Report> DecidedReports { get; set; } = new();
    public List<UserRole> UserRoles { get; set; } = new();
}