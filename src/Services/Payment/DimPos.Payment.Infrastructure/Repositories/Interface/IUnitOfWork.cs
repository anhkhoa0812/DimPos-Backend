using Microsoft.EntityFrameworkCore;

namespace DimPos.Payment.Infrastructure.Repositories.Interface;

public interface IUnitOfWork : IGenericRepositoryFactory, IDisposable
{
    int Commit();

    Task<int> CommitAsync();
}

public interface IUnitOfWork<TContext> : IUnitOfWork where TContext : DbContext
{
    TContext Context { get; }
}