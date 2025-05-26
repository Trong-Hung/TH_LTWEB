using T2_VoTrongHung_2280601119.Models;
using System.Collections.Generic;

namespace T2_VoTrongHung_2280601119.Repository_Pattern
{
    public class MockCategoryRepository : ICategoryRepository
    {
        private List<Category> _categories = new List<Category>
        {
            new Category { Id = 1, Name = "Laptop" },
            new Category { Id = 2, Name = "Điện thoại" },
            new Category { Id = 3, Name = "Đồng hồ" }
        };

        public IEnumerable<Category> GetAllCategories()
        {
            return _categories;
        }

        public Category GetCategoryById(int id)
        {
            // Use FirstOrDefault to avoid exception if not found
            return _categories.FirstOrDefault(c => c.Id == id);
        }
    }
}