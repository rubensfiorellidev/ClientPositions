using Microsoft.EntityFrameworkCore;
using Positions.ConsoleApp.Models;

namespace Positions.ConsoleApp.Data
{
    public sealed class PositionsDbContext : DbContext
    {
        public PositionsDbContext(DbContextOptions<PositionsDbContext> options)
        : base(options) { }

        public DbSet<PositionEntity> Positions => Set<PositionEntity>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<PositionEntity>(entity =>
            {
                entity.ToTable("tb_positions");

                entity.HasKey(x => new { x.PositionId, x.Date });

                entity.Property(x => x.PositionId)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.Property(x => x.ProductId)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.Property(x => x.ClientId)
                    .IsRequired()
                    .HasMaxLength(32);

                entity.Property(x => x.Date)
                    .HasColumnType("date")
                    .IsRequired();

                entity.Property(x => x.Value)
                    .HasColumnName("value")
                    .HasColumnType("numeric(38,18)")
                    .IsRequired();

                entity.Property(x => x.Quantity)
                    .HasColumnName("quantity")
                    .HasColumnType("numeric(38,18)")
                    .IsRequired();

                entity.HasIndex(x => x.ClientId).HasDatabaseName("ix_positions_client");
                entity.HasIndex(x => x.ProductId).HasDatabaseName("ix_positions_product");
                entity.HasIndex(x => x.Date).HasDatabaseName("ix_positions_date");
                entity.HasIndex(x => x.Value).HasDatabaseName("ix_positions_value");

            });

        }
    }
}
