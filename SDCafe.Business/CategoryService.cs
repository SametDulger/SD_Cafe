using SDCafe.DataAccess;
using SDCafe.Entities;

namespace SDCafe.Business
{
    public class CategoryService : Service<Category>, ICategoryService
    {
        public CategoryService(IRepository<Category> repository) : base(repository)
        {
        }

        public override async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _repository.FindAsync(c => !c.IsDeleted);
        }

        public override async Task UpdateAsync(Category entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.UpdatedDate = DateTime.Now;
            await _repository.UpdateAsync(entity);
        }
    }
} 