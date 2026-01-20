using Domain.Entities;

namespace Business.Interfaces.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        public IGenericRepository<T> Repository<T>() where T : BaseEntity;

        Task<int> CommitAsync();
    }
}