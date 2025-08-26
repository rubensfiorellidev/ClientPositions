using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Positions.ConsoleApp.Data;
using Positions.ConsoleApp.Imports;

namespace Positions.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public sealed class PositionsController : ControllerBase
    {
        private readonly PositionsDbContext _db;
        public PositionsController(PositionsDbContext db) => _db = db;

        public sealed record ProductSummaryItem(string ProductId, decimal TotalValue);

        [HttpGet("client/{clientId}")]
        public async Task<ActionResult<List<PositionDto>>> GetClientLatest(
            string clientId, CancellationToken ct = default)
        {
            var latestKeys = _db.Positions
                .Where(p => p.ClientId == clientId)
                .GroupBy(p => p.PositionId)
                .Select(g => new { PositionId = g.Key, Date = g.Max(p => p.Date) });

            var items = await _db.Positions
                .Where(p => p.ClientId == clientId)
                .Join(
                    latestKeys,
                    p => new { p.PositionId, p.Date },
                    k => new { k.PositionId, k.Date },
                    (p, _) => new { p.PositionId, p.ProductId, p.ClientId, p.Date, p.Value, p.Quantity }
                )
                .OrderBy(x => x.PositionId).ThenByDescending(x => x.Date)
                .Select(x => new PositionDto(x.PositionId, x.ProductId, x.ClientId, x.Date, x.Value, x.Quantity))
                .ToListAsync(ct);

            return Ok(items);
        }

        [HttpGet("client/{clientId}/summary")]
        public async Task<ActionResult<List<ProductSummaryItem>>> GetClientLatestSummary(string clientId, CancellationToken ct)
        {
            var latestKeys = _db.Positions
                .Where(p => p.ClientId == clientId)
                .GroupBy(p => p.PositionId)
                .Select(g => new { PositionId = g.Key, Date = g.Max(p => p.Date) });

            var latestRows = _db.Positions
                .Where(p => p.ClientId == clientId)
                .Join(
                    latestKeys,
                    p => new { p.PositionId, p.Date },
                    k => new { k.PositionId, k.Date },
                    (p, _) => new { p.ProductId, p.Value }
                );

            var summary = await latestRows
                .GroupBy(x => x.ProductId)
                .Select(g => new { ProductId = g.Key, TotalValue = g.Sum(v => v.Value) })
                .OrderByDescending(x => x.TotalValue)
                .Select(x => new ProductSummaryItem(x.ProductId, x.TotalValue))
                .ToListAsync(ct);

            return Ok(summary);
        }

        [HttpGet("top10")]
        [ResponseCache(Duration = 30, Location = ResponseCacheLocation.Any, NoStore = false)]
        public async Task<ActionResult<List<PositionDto>>> GetTop10LatestByValue(CancellationToken stoppingToken)
        {
            var latestKeys = _db.Positions
                .GroupBy(p => p.PositionId)
                .Select(g => new { g.Key, MaxDate = g.Max(p => p.Date) });

            var query = _db.Positions
                .Join(
                    latestKeys,
                    p => new { p.PositionId, p.Date },
                    k => new { PositionId = k.Key, Date = k.MaxDate },
                    (p, _) => new
                    {
                        p.PositionId,
                        p.ProductId,
                        p.ClientId,
                        p.Date,
                        p.Value,
                        p.Quantity
                    });

            var top = await query
                .OrderByDescending(x => x.Value)
                .ThenByDescending(x => x.Date)
                .Take(10)
                .Select(x => new PositionDto(
                    x.PositionId, x.ProductId, x.ClientId, x.Date, x.Value, x.Quantity))
                .ToListAsync(stoppingToken);

            return Ok(top);
        }


    }
}
