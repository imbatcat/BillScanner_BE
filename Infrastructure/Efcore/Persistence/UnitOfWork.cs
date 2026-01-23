using Business.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.MarkerInterfaces;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Concurrent;
using JetBrains.Annotations;

namespace Infrastructure.Efcore.Persistence
{
    [UsedImplicitly]
    public class UnitOfWork(BillScannerDbContext dbContext) : IUnitOfWork, IScopedService
    {
        // ===================================
        // === Fields & Prop
        // ===================================

        private readonly ConcurrentDictionary<string, object> _repositories = new();
        private bool _disposed;

        // ===================================
        // === Methods
        // ===================================

        /// <summary>
        ///     Dispose object
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // dbContext is resolved from DI, so we should allow DI container to dispose it.
                    // dbContext.Dispose(); 
                }

                _disposed = true;
            }
        }

        ~UnitOfWork()
        {
            Dispose(false);
        }

        public IExecutionStrategy CreateExecutionStrategy() => dbContext.Database.CreateExecutionStrategy();

        public async Task<int> CommitAsync() => await dbContext.SaveChangesAsync();

        public IGenericRepository<T> Repository<T>() where T : BaseEntity
        {
            var typeEntityName = typeof(T).Name;

            var repoInstanceTypeT = _repositories.GetOrAdd(typeEntityName,
                valueFactory: _ =>
                {
                    var repoType = typeof(GenericRepository<T>);

                    var repoInstance = Activator.CreateInstance(repoType, dbContext);

                    return repoInstance!;
                });

            return (IGenericRepository<T>)repoInstanceTypeT;
        }
    }
}