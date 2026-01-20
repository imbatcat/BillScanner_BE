using Business.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Concurrent;

namespace Infrastructure.Efcore.Persistence
{
    internal static class RepositoryDictionary
    {
        private static readonly ConcurrentDictionary<string, object> _repositoryDictionary = new();

        public static ConcurrentDictionary<string, object> GetRepositoryDictionary() => _repositoryDictionary;
    }

    internal class UnitOfWork(BillScannerDbContext dbContext) : IUnitOfWork
    {
        // ===================================
        // === Fields & Prop
        // ===================================

        private bool _disposed = false;

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
                    dbContext.Dispose();
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

            var repoInstanceTypeT = RepositoryDictionary.GetRepositoryDictionary().GetOrAdd(typeEntityName,
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