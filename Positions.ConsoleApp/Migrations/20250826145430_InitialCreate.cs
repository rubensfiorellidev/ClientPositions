using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Positions.ConsoleApp.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tb_positions",
                columns: table => new
                {
                    PositionId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    ProductId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ClientId = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    value = table.Column<decimal>(type: "numeric(38,18)", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(38,18)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_positions", x => new { x.PositionId, x.Date });
                });

            migrationBuilder.CreateIndex(
                name: "ix_positions_client",
                table: "tb_positions",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "ix_positions_date",
                table: "tb_positions",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "ix_positions_product",
                table: "tb_positions",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "ix_positions_value",
                table: "tb_positions",
                column: "value");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tb_positions");
        }
    }
}
