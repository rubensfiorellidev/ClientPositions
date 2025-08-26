using Positions.ConsoleApp.Imports;

namespace Positions.ConsoleApp.Contracts
{
    public interface IPositionsSource
    {
        IAsyncEnumerable<PositionDto> StreamAsync(CancellationToken stoppingToken);
    }
}
