using System;
using System.Collections.Generic;

namespace Kartverket.Web.Data.EF;

public partial class User
{
    public int UserID { get; set; }

    public int OrgID { get; set; }

    public string? Email { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordSalt { get; set; } = null!;

    public virtual Organisation Org { get; set; } = null!;

    public virtual ICollection<Report> ReportAssignedToUserNavigations { get; set; } = new List<Report>();

    public virtual ICollection<Report> ReportDecisionByUserNavigations { get; set; } = new List<Report>();

    public virtual ICollection<Report> ReportUsers { get; set; } = new List<Report>();

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
