using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoTrongHung2280601119.Models;
using VoTrongHung2280601119.Repositories;

namespace VoTrongHung2280601119.Repositories
{
    public class EFOrderItemRepository : IOrderItemRepository
    {
        private readonly ApplicationDbContext _context;

        public EFOrderItemRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OrderItem>> GetAllAsync()
        {
            return await _context.OrderItems.Include(oi => oi.Product).Include(oi => oi.Warehouse).ToListAsync();
        }

        public async Task<OrderItem> GetByIdAsync(int id)
        {
            return await _context.OrderItems.Include(oi => oi.Product).Include(oi => oi.Warehouse).FirstOrDefaultAsync(oi => oi.Id == id);
        }

        public async Task AddAsync(OrderItem orderItem)
        {
            _context.OrderItems.Add(orderItem);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(OrderItem orderItem)
        {
            _context.OrderItems.Update(orderItem);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var orderItem = await _context.OrderItems.FindAsync(id);
            if (orderItem != null)
            {
                _context.OrderItems.Remove(orderItem);
                await _context.SaveChangesAsync();
            }
        }
    }
}