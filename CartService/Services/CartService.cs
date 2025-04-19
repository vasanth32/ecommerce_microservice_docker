using CartService.Models;

namespace CartService.Services;

public class CartManager
{
    private readonly Dictionary<string, List<CartItem>> _carts = new();

    public List<CartItem> GetCart(string userId)
    {
        return _carts.GetValueOrDefault(userId, new List<CartItem>());
    }

    public void AddItem(string userId, CartItem item)
    {
        if (!_carts.ContainsKey(userId))
        {
            _carts[userId] = new List<CartItem>();
        }

        var existingItem = _carts[userId].FirstOrDefault(i => i.ProductId == item.ProductId);
        if (existingItem != null)
        {
            existingItem.Quantity += item.Quantity;
        }
        else
        {
            _carts[userId].Add(item);
        }
    }

    public bool RemoveItem(string userId, string productId)
    {
        if (!_carts.ContainsKey(userId))
        {
            return false;
        }

        var item = _carts[userId].FirstOrDefault(i => i.ProductId == productId);
        if (item == null)
        {
            return false;
        }

        return _carts[userId].Remove(item);
    }
} 