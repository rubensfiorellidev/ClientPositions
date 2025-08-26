namespace Positions.ConsoleApp.Contracts
{
    public interface IPositionsImporter
    {
        Task<long> ImportAsync(CancellationToken stoppingToken);
    }
}
