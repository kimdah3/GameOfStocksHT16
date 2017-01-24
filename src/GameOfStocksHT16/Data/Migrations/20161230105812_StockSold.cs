using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace GameOfStocksHT16.Data.Migrations
{
    public partial class StockSold : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPending",
                table: "StockTransaction");

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

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "StockTransaction",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_StockSold_UserId",
                table: "StockSold",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "StockTransaction");

            migrationBuilder.DropTable(
                name: "StockSold");

            migrationBuilder.AddColumn<bool>(
                name: "IsPending",
                table: "StockTransaction",
                nullable: false,
                defaultValue: false);
        }
    }
}
