using System.Text.Json;
using Web_Ban_Laptop.Models;
using Web_Ban_Laptop.Models.ViewModels;

namespace Web_Ban_Laptop.Services;

public class CartService
{
    private const string CartSessionKey = "Cart";

    public List<CartItemViewModel> GetCart(ISession session)
    {
        var cartJson = session.GetString(CartSessionKey);
        if (string.IsNullOrEmpty(cartJson))
            return new List<CartItemViewModel>();

        return JsonSerializer.Deserialize<List<CartItemViewModel>>(cartJson) ?? new List<CartItemViewModel>();
    }

    public void SaveCart(ISession session, List<CartItemViewModel> cart)
    {
        var cartJson = JsonSerializer.Serialize(cart);
        session.SetString(CartSessionKey, cartJson);
    }

    public void AddToCart(ISession session, Product product, int quantity = 1)
    {
        var cart = GetCart(session);
        var existingItem = cart.FirstOrDefault(x => x.ProductId == product.ProductId);

        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            cart.Add(new CartItemViewModel
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                Thumbnail = product.Thumbnail,
                Price = product.Price,
                DiscountPrice = product.DiscountPrice,
                Quantity = quantity
            });
        }

        SaveCart(session, cart);
    }

    public void UpdateCartItem(ISession session, int productId, int quantity)
    {
        var cart = GetCart(session);
        var item = cart.FirstOrDefault(x => x.ProductId == productId);
        if (item != null)
        {
            if (quantity <= 0)
                cart.Remove(item);
            else
            {
                item.Quantity = quantity;
            }
        }
        SaveCart(session, cart);
    }

    public void RemoveFromCart(ISession session, int productId)
    {
        var cart = GetCart(session);
        cart.RemoveAll(x => x.ProductId == productId);
        SaveCart(session, cart);
    }

    public void ClearCart(ISession session)
    {
        session.Remove(CartSessionKey);
    }

    public int GetCartCount(ISession session)
    {
        return GetCart(session).Sum(x => x.Quantity);
    }

    public decimal GetCartTotal(ISession session)
    {
        return GetCart(session).Sum(x => x.SubTotal);
    }
}

