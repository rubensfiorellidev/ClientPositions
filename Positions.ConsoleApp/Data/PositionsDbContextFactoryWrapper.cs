using Microsoft.EntityFrameworkCore;
using Positions.ConsoleApp.Contracts;

namespace Positions.ConsoleApp.Data
{
    public sealed class PositionsDbContextFactoryWrapper : IPositionsDbContextFactory
    {
        private readonly IDbContextFactory<PositionsDbContext> _factory;
        public PositionsDbContextFactoryWrapper(IDbContextFactory<PositionsDbContext> factory)
            => _factory = factory;

        public PositionsDbContext CreateDbContext() => _factory.CreateDbContext();
    }
}
