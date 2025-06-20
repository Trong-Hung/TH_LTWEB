using System.Collections.Generic;
using System.Linq;
using VoTrongHung2280601119.Models;

namespace VoTrongHung2280601119.Models
{
    public class ShoppingCart
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();

        public void AddItem(CartItem item)
        {
            var existingItem = Items.FirstOrDefault(i => i.ProductId == item.ProductId && i.WarehouseId == item.WarehouseId);

            if (existingItem != null)
            {
                existingItem.Quantity += item.Quantity;
            }
            else
            {
                Items.Add(item);
            }
        }

        public void RemoveItem(int productId, int warehouseId)
        {
            Items.RemoveAll(i => i.ProductId == productId && i.WarehouseId == warehouseId);
        }

        public decimal CalculateTotal()
        {
            return Items.Sum(item => item.Subtotal);
        }

        public void Clear()
        {
            Items.Clear();
        }
    }
}