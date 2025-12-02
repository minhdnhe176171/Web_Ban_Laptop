using System;
using System.Collections.Generic;

namespace Web_Ban_Laptop.Models;

public partial class InventoryLog
{
    public int LogId { get; set; }

    public int ProductId { get; set; }

    public int ChangeAmount { get; set; }

    public string? Reason { get; set; }

    public DateTime? ActionDate { get; set; }

    public int? UserId { get; set; }

    public virtual Product Product { get; set; } = null!;
}
