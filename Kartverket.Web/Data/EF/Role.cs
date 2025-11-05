using System;
using System.Collections.Generic;

namespace Kartverket.Web.Data.EF;

public partial class Role
{
    public int RoleID { get; set; }

    public string RoleName { get; set; } = null!;

    public string? RoleDescription { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
