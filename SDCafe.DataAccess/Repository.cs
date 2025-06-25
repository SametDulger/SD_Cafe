using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace SDCafe.DataAccess
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly SDCafeDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(SDCafeDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<T?> GetByIdWithIncludesAsync(int id, params Expression<Func<T, object>>[] includes)
        {
            var query = _dbSet.AsQueryable();
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            return await query.FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id);
        }

        public async Task<T?> GetByIdWithNestedIncludesAsync(int id, params string[] includes)
        {
            var query = _dbSet.AsQueryable();
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            return await query.FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAllWithIncludesAsync(params Expression<Func<T, object>>[] includes)
        {
            var query = _dbSet.AsQueryable();
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            return await query.ToListAsync();
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public async Task<IEnumerable<T>> FindWithIncludesAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
        {
            var query = _dbSet.AsQueryable();
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            return await query.Where(predicate).ToListAsync();
        }

        public async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task UpdateAsync(T entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                _dbSet.Update(entity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Entity güncellenirken hata oluştu: {ex.Message}", ex);
            }
        }

        public async Task UpdateSpecificFieldsAsync(T entity, params Expression<Func<T, object>>[] properties)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                var idProperty = typeof(T).GetProperty("Id");
                if (idProperty == null)
                    throw new InvalidOperationException("Entity'de Id property'si bulunamadı.");
                
                var entityId = (int)idProperty.GetValue(entity)!;
                
                var existingEntity = await _dbSet.FindAsync(entityId);
                if (existingEntity == null)
                    throw new InvalidOperationException($"ID {entityId} ile entity bulunamadı.");

                foreach (var property in properties)
                {
                    string propertyName = "";
                    if (property.Body is MemberExpression memberExpression)
                    {
                        propertyName = memberExpression.Member.Name;
                    }
                    else if (property.Body is UnaryExpression unaryExpression && 
                             unaryExpression.Operand is MemberExpression memberExpr)
                    {
                        propertyName = memberExpr.Member.Name;
                    }

                    if (!string.IsNullOrEmpty(propertyName))
                    {
                        var propertyInfo = typeof(T).GetProperty(propertyName);
                        if (propertyInfo != null)
                        {
                            var newValue = propertyInfo.GetValue(entity);
                            propertyInfo.SetValue(existingEntity, newValue);
                        }
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Entity güncellenirken hata oluştu: {ex.Message}", ex);
            }
        }

        public async Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.CountAsync(predicate);
        }

        public void DetachEntity(T entity)
        {
            var entry = _context.Entry(entity);
            if (entry != null)
            {
                entry.State = EntityState.Detached;
            }
        }
    }
} 