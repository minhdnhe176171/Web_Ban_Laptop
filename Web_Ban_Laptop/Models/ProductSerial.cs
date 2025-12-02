using System;
using System.Collections.Generic;

namespace Web_Ban_Laptop.Models;

public partial class ProductSerial
{
    public int SerialId { get; set; }

    public int ProductId { get; set; }

    public string SerialNumber { get; set; } = null!;

    public int? Status { get; set; }

    public virtual Product Product { get; set; } = null!;
}
