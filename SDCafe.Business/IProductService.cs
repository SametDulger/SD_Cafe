using SDCafe.Entities;

namespace SDCafe.Business
{
    public interface IProductService : IService<Product>
    {
        Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId);
        Task<IEnumerable<Product>> SearchAsync(string searchTerm);
    }
} 