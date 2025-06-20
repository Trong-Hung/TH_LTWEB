using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoTrongHung2280601119.Models;
using VoTrongHung2280601119.Repositories;

namespace VoTrongHung2280601119.Repositories
{
    public class EFOrderDistributionRepository : IOrderDistributionRepository
    {
        private readonly ApplicationDbContext _context;

        public EFOrderDistributionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OrderDistribution>> GetAllAsync()
        {
            return await _context.OrderDistributions.Include(o => o.Customer).Include(o => o.OrderItems).ThenInclude(oi => oi.Product).ToListAsync();
        }

        public async Task<OrderDistribution> GetByIdAsync(int id)
        {
            return await _context.OrderDistributions.Include(o => o.Customer).Include(o => o.OrderItems).ThenInclude(oi => oi.Product).FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task AddAsync(OrderDistribution order)
        {
            _context.OrderDistributions.Add(order);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(OrderDistribution order)
        {
            _context.OrderDistributions.Update(order);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var order = await _context.OrderDistributions.FindAsync(id);
            if (order != null)
            {
                _context.OrderDistributions.Remove(order);
                await _context.SaveChangesAsync();
            }
        }
    }
}