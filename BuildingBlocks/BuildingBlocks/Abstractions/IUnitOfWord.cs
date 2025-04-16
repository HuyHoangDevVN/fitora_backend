using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Abstractions;

public interface IUnitOfWork : IDisposable
{
    Task<int> CommitAsync();
}

public interface IUnitOfWork<TContext> : IUnitOfWork where TContext : DbContext
{}