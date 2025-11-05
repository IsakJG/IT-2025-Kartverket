using System;
using System.Collections.Generic;

namespace Kartverket.Web.Data.EF;

public partial class Organisation
{
    public int OrgID { get; set; }

    public string OrgName { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
