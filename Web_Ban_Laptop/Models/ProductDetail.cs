using System;
using System.Collections.Generic;

namespace Web_Ban_Laptop.Models;

public partial class ProductDetail
{
    public int DetailId { get; set; }

    public int ProductId { get; set; }

    public string? Cpu { get; set; }

    public string? Ram { get; set; }

    public string? HardDrive { get; set; }

    public string? Screen { get; set; }

    public string? Vga { get; set; }

    public string? Os { get; set; }

    public string? Battery { get; set; }

    public double? Weight { get; set; }

    public string? Warranty { get; set; }

    public virtual Product Product { get; set; } = null!;
}
