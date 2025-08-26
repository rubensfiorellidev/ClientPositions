using Positions.ConsoleApp.Data;

namespace Positions.ConsoleApp.Contracts
{
    public interface IPositionsDbContextFactory
    {
        PositionsDbContext CreateDbContext();
    }
}
