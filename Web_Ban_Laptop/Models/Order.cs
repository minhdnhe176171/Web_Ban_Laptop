using System;
using System.Collections.Generic;

namespace Web_Ban_Laptop.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int? UserId { get; set; }

    public DateTime? OrderDate { get; set; }

    public int StatusId { get; set; }

    public decimal TotalAmount { get; set; }

    public int? VoucherId { get; set; }

    public string ShippingAddress { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string? Note { get; set; }

    public string? PaymentMethod { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual OrderStatus Status { get; set; } = null!;

    public virtual User? User { get; set; }

    public virtual Voucher? Voucher { get; set; }
}
