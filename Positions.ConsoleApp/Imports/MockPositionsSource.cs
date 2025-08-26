using Positions.ConsoleApp.Contracts;
using System.Runtime.CompilerServices;

namespace Positions.ConsoleApp.Imports
{
    public sealed class MockPositionsSource : IPositionsSource
    {
        private readonly int _count, _positions, _products, _clients, _days, _seed;

        public MockPositionsSource(int total = 25_000, int positions = 10_000, int products = 100, int clients = 300, int days = 365, int seed = 42)
        {
            _count = total; _positions = positions; _products = products; _clients = clients; _days = days; _seed = seed;
        }

        public async IAsyncEnumerable<PositionDto> StreamAsync([EnumeratorCancellation] CancellationToken stoppingToken)
        {
            var rnd = new Random(_seed);
            for (int i = 0; i < _count; i++)
            {
                stoppingToken.ThrowIfCancellationRequested();

                yield return new PositionDto(
                    PositionId: $"pos-{i % _positions}",
                    ProductId: $"prd-{i % _products}",
                    ClientId: $"000.000.000-{i % _clients:D2}",
                    Date: DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-(i % _days))),
                    Value: (decimal)(rnd.NextDouble() * 100_000),
                    Quantity: (decimal)(rnd.NextDouble() * 10_000)
                );

                if (i % 10_000 == 0) await Task.Yield();
            }
        }
    }
}
