using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Positions.ConsoleApp.Contracts;
using Positions.ConsoleApp.Data;
using Positions.ConsoleApp.ExternalServices;
using Positions.ConsoleApp.Models;

namespace Positions.ConsoleApp.Imports
{
    public sealed class PositionsImporter : IPositionsImporter
    {
        private readonly IDbContextFactory<PositionsDbContext> _dbFactory;
        private readonly IPositionsSource _source;
        private readonly ILogger<PositionsImporter> _log;
        private readonly ExternalApiOptions _opts;

        public PositionsImporter(
            IDbContextFactory<PositionsDbContext> dbFactory,
            IPositionsSource source,
            IOptions<ExternalApiOptions> options,
            ILogger<PositionsImporter> log)
        {
            _dbFactory = dbFactory;
            _source = source;
            _opts = options.Value;
            _log = log;
        }
        public async Task<long> ImportAsync(CancellationToken stoppingToken)
        {
            _log.LogInformation("Import start. batch={Batch}", _opts.BatchSize);

            var started = DateTime.UtcNow;
            long total = 0;
            long seen = 0;

            await using var db = await _dbFactory.CreateDbContextAsync(stoppingToken);

            await db.Database.MigrateAsync(stoppingToken);

            if (_opts.TruncateBeforeImport && !_opts.UseUpsert)
            {
                await db.Database.ExecuteSqlRawAsync("TRUNCATE TABLE tb_positions;", stoppingToken);
            }

            db.ChangeTracker.AutoDetectChangesEnabled = false;
            db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            var batch = new List<PositionEntity>(_opts.BatchSize);

            await foreach (var dto in _source.StreamAsync(stoppingToken))
            {
                batch.Add(new PositionEntity(dto.PositionId, dto.ProductId, dto.ClientId, dto.Date, dto.Value, dto.Quantity));
                seen++;

                if (batch.Count >= _opts.BatchSize)
                {
                    total += await FlushAsync(db, batch, stoppingToken);

                    if (total % 100_000 == 0)
                        _log.LogInformation("Progress: {Total} rows inserted...", total);
                }

                if (_opts.MaxItems is not null && seen >= _opts.MaxItems.Value)
                    break;
            }

            if (batch.Count > 0)
                total += await FlushAsync(db, batch, stoppingToken, _opts.UseUpsert);

            var elapsed = DateTime.UtcNow - started;
            var rps = elapsed.TotalSeconds > 0 ? total / elapsed.TotalSeconds : total;
            _log.LogInformation("Import done. rows={Total} in {Seconds:n1}s (~{Rps:n0} rows/s)",
                total, elapsed.TotalSeconds, rps);

            return total;
        }

        private static async Task<long> FlushAsync(PositionsDbContext db, List<PositionEntity> batch, CancellationToken ct, bool useUpsert = false)
        {
            if (batch.Count == 0) return 0;

            if (!useUpsert)
            {
                db.Positions.AddRange(batch);
                await db.SaveChangesAsync(ct);
                var n0 = batch.Count;
                batch.Clear();
                db.ChangeTracker.Clear();
                return n0;
            }

            var sb = new System.Text.StringBuilder();
            var parms = new List<object>(batch.Count * 6);

            sb.Append("""
                INSERT INTO tb_positions ("Date","PositionId","ClientId","ProductId",value,quantity)
                VALUES
                """);

            for (int i = 0; i < batch.Count; i++)
            {
                var p = batch[i];
                if (i > 0) sb.Append(',');

                sb.Append($"(@d{i},@pos{i},@cli{i},@prod{i},@val{i},@qty{i})");

                parms.Add(new Npgsql.NpgsqlParameter($"d{i}", p.Date));
                parms.Add(new Npgsql.NpgsqlParameter($"pos{i}", p.PositionId));
                parms.Add(new Npgsql.NpgsqlParameter($"cli{i}", p.ClientId));
                parms.Add(new Npgsql.NpgsqlParameter($"prod{i}", p.ProductId));
                parms.Add(new Npgsql.NpgsqlParameter($"val{i}", p.Value));
                parms.Add(new Npgsql.NpgsqlParameter($"qty{i}", p.Quantity));
            }

            sb.Append("""
                ON CONFLICT ("PositionId","Date") DO UPDATE SET
                "ClientId" = EXCLUDED."ClientId",
                "ProductId" = EXCLUDED."ProductId",
                value      = EXCLUDED.value,
                quantity   = EXCLUDED.quantity;
                """);

            var sql = sb.ToString();
            await db.Database.ExecuteSqlRawAsync(sql, parms.ToArray(), ct);

            var n = batch.Count;
            batch.Clear();
            db.ChangeTracker.Clear();
            return n;
        }
    }
}
