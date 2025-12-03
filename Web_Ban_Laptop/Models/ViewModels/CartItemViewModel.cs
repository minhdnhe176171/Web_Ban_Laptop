namespace Web_Ban_Laptop.Models.ViewModels;

public class CartItemViewModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string? Thumbnail { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int Quantity { get; set; }
    
    public decimal FinalPrice => DiscountPrice ?? Price;
    public decimal SubTotal => FinalPrice * Quantity;
}

