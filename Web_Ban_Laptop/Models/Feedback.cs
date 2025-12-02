using System;
using System.Collections.Generic;

namespace Web_Ban_Laptop.Models;

public partial class Feedback
{
    public int FeedbackId { get; set; }

    public int UserId { get; set; }

    public int ProductId { get; set; }

    public int? Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime? FeedbackDate { get; set; }

    public string? Reply { get; set; }

    public int? RepliedBy { get; set; }

    public DateTime? RepliedDate { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
