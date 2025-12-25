using Microsoft.EntityFrameworkCore;
using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Infrastructure.ApplicationDBContext;
using System.Linq.Expressions;

namespace EduocationSystem.Infrastructure.Repositories
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<TEntity> _dbSet;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<TEntity>();
        }

        // Add a single entity to the database
        public virtual async Task AddAsync(TEntity entity)
        {
            await _dbSet.AddAsync(entity);
        }

        // Add multiple entities to the database
        public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        // Count entities with optional criteria
        public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>>? criteria = null)
        {
            return criteria == null
                ? await _dbSet.CountAsync()
                : await _dbSet.CountAsync(criteria);
        }

        // Delete a single entity
        public virtual void Delete(TEntity entity)
        {
            // i want to access the base entity and set its IsDeleted property to true instead of removing it from the database
            _dbSet.Attach(entity);
            //detedat = time.now
            _context.Entry(entity).Property("IsDeleted").CurrentValue = true;
            _context.Entry(entity).Property("DeletedAt").CurrentValue = DateTime.UtcNow;
        }

        public virtual void HardDelete(TEntity entity)
        {
            // if the isdeleted = false then use delete method without removing it from the database
            var isDeleted = (bool)_context.Entry(entity).Property("IsDeleted").CurrentValue;
            if (isDeleted)
                _dbSet.Remove(entity);
            else
                Delete(entity);
        }

        // Delete multiple entities
        public virtual void DeleteRange(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                _dbSet.Attach(entity);
                _context.Entry(entity).Property("IsDeleted").CurrentValue = true;
            }
        }

        // Get the first entity matching criteria (with optional includes)
        public virtual async Task<TEntity?> FirstOrDefaultAsync(
            Expression<Func<TEntity, bool>> criteria,
            params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = _dbSet;

            // Apply includes if provided
            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            return await query.FirstOrDefaultAsync(criteria);
        }

        // Get all entities (with optional includes)
        public virtual IQueryable<TEntity> GetAll()
        {
            return _dbSet;
        }

        // Get an entity by its primary key (with optional includes)
        public virtual async Task<TEntity?> GetByIdAsync(
            int id,
            params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = _dbSet;

            // Apply includes if provided
            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            return await query.FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id);
        }

        // Update an entity
        public virtual void Update(TEntity entity)
        {
            _dbSet.Update(entity);
        }
    }
}
