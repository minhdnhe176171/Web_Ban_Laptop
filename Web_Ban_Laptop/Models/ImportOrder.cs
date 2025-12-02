using System;
using System.Collections.Generic;

namespace Web_Ban_Laptop.Models;

public partial class ImportOrder
{
    public int ImportId { get; set; }

    public int SupplierId { get; set; }

    public int ManagerId { get; set; }

    public DateTime? ImportDate { get; set; }

    public decimal? TotalAmount { get; set; }

    public string? Note { get; set; }

    public virtual ICollection<ImportOrderDetail> ImportOrderDetails { get; set; } = new List<ImportOrderDetail>();

    public virtual User Manager { get; set; } = null!;

    public virtual Supplier Supplier { get; set; } = null!;
}
