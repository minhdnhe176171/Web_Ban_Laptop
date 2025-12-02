using System;
using System.Collections.Generic;

namespace Web_Ban_Laptop.Models;

public partial class ImportOrderDetail
{
    public int ImportDetailId { get; set; }

    public int ImportId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal ImportPrice { get; set; }

    public virtual ImportOrder Import { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
