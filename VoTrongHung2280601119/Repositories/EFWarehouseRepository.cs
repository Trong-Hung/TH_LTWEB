using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoTrongHung2280601119.Models;
using VoTrongHung2280601119.Repositories;

namespace VoTrongHung2280601119.Repositories
{
    public class EFWarehouseRepository : IWarehouseRepository
    {
        private readonly ApplicationDbContext _context;

        public EFWarehouseRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Warehouse>> GetAllAsync()
        {
            return await _context.Warehouses.ToListAsync();
        }

        public async Task<Warehouse> GetByIdAsync(int id)
        {
            return await _context.Warehouses.FindAsync(id);
        }

        public async Task<Warehouse> GetBySlugAsync(string slug)
        {
            return await _context.Warehouses.FirstOrDefaultAsync(w => w.Slug == slug);
        }

        public async Task AddAsync(Warehouse warehouse)
        {
            _context.Warehouses.Add(warehouse);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Warehouse warehouse)
        {
            _context.Warehouses.Update(warehouse);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var warehouse = await _context.Warehouses.FindAsync(id);
            if (warehouse != null)
            {
                _context.Warehouses.Remove(warehouse);
                await _context.SaveChangesAsync();
            }
        }
    }
}