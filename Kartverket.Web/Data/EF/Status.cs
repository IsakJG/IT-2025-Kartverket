using System;
using System.Collections.Generic;

namespace Kartverket.Web.Data.EF;

public partial class Status
{
    public int StatusID { get; set; }

    public string StatusName { get; set; } = null!;

    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
}
