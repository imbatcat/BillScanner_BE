using AutoMapper;
using AutoMapper.QueryableExtensions;
using Business.Interfaces.Repositories;
using Business.Interfaces.Specifications;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    internal class GenericRepository<T>(DbContext context) : IGenericRepository<T> where T : BaseEntity
    {
        protected readonly DbContext _context = context;

        protected readonly DbSet<T> _dbSet = context.Set<T>();

        public virtual async Task<T?> GetByIdAsync(Guid id, bool asNoTracking = true, List<string>? includes = null)
        {
            IQueryable<T> query = _dbSet;

            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync(e => e.Id == id);
        }

        public virtual async Task<IReadOnlyList<T>> GetAllWithSpecificationAsync(ISpecification<T> specification, bool asNoTracking = true)
        {
            var query = ApplySpecification(specification, asNoTracking);
            return await query.ToListAsync();
        }

        public virtual async Task<T?> GetBySpecificationAsync(ISpecification<T> specification, bool asNoTracking = true)
        {
            var query = ApplySpecification(specification, asNoTracking);
            return await query.FirstOrDefaultAsync();
        }

        public virtual async Task<int> CountAsync(ISpecification<T> specification)
        {
            var query = ApplySpecification(specification, true);
            return await query.CountAsync();
        }

        public virtual async Task<bool> AnyAsync(ISpecification<T> specification)
        {
            var query = ApplySpecification(specification, true);
            return await query.AnyAsync();
        }

        public virtual async Task<TDto?> GetBySpecificationProjectedAsync<TDto>(ISpecification<T> specification, IConfigurationProvider mapperConfig)
        {
            var query = ApplySpecification(specification, true);
            return await query.ProjectTo<TDto>(mapperConfig).FirstOrDefaultAsync();
        }

        public virtual async Task<TDto?> GetByIdProjectedAsync<TDto>(Guid id, IConfigurationProvider mapperConfig)
        {
            return await _dbSet
                .Where(e => e.Id == id)
                .ProjectTo<TDto>(mapperConfig)
                .FirstOrDefaultAsync();
        }

        public virtual async Task<IReadOnlyList<TDto>> GetAllWithSpecificationProjectedAsync<TDto>(ISpecification<T> specification, IConfigurationProvider mapperConfig)
        {
            var query = ApplySpecification(specification, true);
            return await query.ProjectTo<TDto>(mapperConfig).ToListAsync();
        }

        public virtual T Insert(T entity)
        {
            _dbSet.Add(entity);
            return entity;
        }

        public virtual void InsertRange(List<T> entities)
        {
            _dbSet.AddRange(entities);
        }

        public virtual T? Update(T entityToUpdate)
        {
            if (_context.Entry(entityToUpdate).State == EntityState.Detached)
            {
                _dbSet.Attach(entityToUpdate);
            }
            _context.Entry(entityToUpdate).State = EntityState.Modified;
            return entityToUpdate;
        }

        public virtual T? Delete(T entityToDelete)
        {
            if (_context.Entry(entityToDelete).State == EntityState.Detached)
            {
                _dbSet.Attach(entityToDelete);
            }
            _dbSet.Remove(entityToDelete);
            return entityToDelete;
        }

        public virtual T? Delete(object id)
        {
            var entityToDelete = _dbSet.Find(id);
            if (entityToDelete != null)
            {
                Delete(entityToDelete);
            }
            return entityToDelete;
        }

        public virtual T? SoftDelete(T entityToDelete)
        {
            return Update(entityToDelete);
        }

        public virtual T? SoftDelete(object id)
        {
            var entityToDelete = _dbSet.Find(id);
            if (entityToDelete != null)
            {
                return SoftDelete(entityToDelete);
            }
            return entityToDelete;
        }

        private IQueryable<T> ApplySpecification(ISpecification<T> specification, bool asNoTracking)
        {
            var query = SpecificationEvaluator<T>.GetQuery(_dbSet.AsQueryable(), specification);

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            return query;
        }
    }
}