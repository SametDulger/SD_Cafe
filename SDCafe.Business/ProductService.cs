using SDCafe.DataAccess;
using SDCafe.Entities;
using System.Linq.Expressions;

namespace SDCafe.Business
{
    public class ProductService : Service<Product>, IProductService
    {
        public ProductService(IRepository<Product> repository) : base(repository)
        {
        }

        public override async Task UpdateAsync(Product entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.UpdatedDate = DateTime.Now;
            
            await _repository.UpdateAsync(entity);
        }

        public async Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId)
        {
            return await _repository.FindAsync(p => p.CategoryId == categoryId && !p.IsDeleted);
        }

        public async Task<IEnumerable<Product>> SearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            return await _repository.FindAsync(p => 
                (p.Name.Contains(searchTerm) || (p.Description != null && p.Description.Contains(searchTerm))) && !p.IsDeleted);
        }

        public override async Task<Product?> GetByIdWithIncludesAsync(int id, params Expression<Func<Product, object>>[] includes)
        {
            var result = await _repository.GetByIdWithIncludesAsync(id, includes);
            return result;
        }

        public override async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _repository.FindAsync(p => !p.IsDeleted);
        }
    }
} 