using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace GameOfStocksHT16.Data.Migrations
{
    public partial class NewPropertiesForStockOwnership : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateBought",
                table: "StockOwnership");

            migrationBuilder.DropColumn(
                name: "Ask",
                table: "StockOwnership");

            migrationBuilder.DropTable(
                name: "StockSold");

            migrationBuilder.AddColumn<decimal>(
                name: "Gav",
                table: "StockOwnership",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalSum",
                table: "StockOwnership",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Gav",
                table: "StockOwnership");

            migrationBuilder.DropColumn(
                name: "TotalSum",
                table: "StockOwnership");

            migrationBuilder.CreateTable(
                name: "StockSold",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DateSold = table.Column<DateTime>(nullable: false),
                    Label = table.Column<string>(nullable: true),
                    LastTradePrice = table.Column<decimal>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Quantity = table.Column<int>(nullable: false),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockSold", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockSold_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddColumn<DateTime>(
                name: "Ask",
                table: "StockOwnership",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "PriceBought",
                table: "StockOwnership",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_StockSold_UserId",
                table: "StockSold",
                column: "UserId");
        }
    }
}
