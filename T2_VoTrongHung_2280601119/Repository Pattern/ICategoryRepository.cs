using T2_VoTrongHung_2280601119.Models;

namespace T2_VoTrongHung_2280601119.Repository_Pattern
{
    public interface ICategoryRepository
    {
        IEnumerable<Category> GetAllCategories();
    }
}
