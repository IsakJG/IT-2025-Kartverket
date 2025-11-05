using System;
using System.Collections.Generic;

namespace Kartverket.Web.Data.EF;

public partial class Category
{
    public int CategoryID { get; set; }

    public string CategoryName { get; set; } = null!;

    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
}
