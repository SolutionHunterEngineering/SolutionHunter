using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Observer.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddHunterServers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HunterServers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    BaseUrl = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    IsRunning = table.Column<bool>(type: "bit", nullable: false),
                    WorkCapacity = table.Column<int>(type: "int", nullable: false),
                    CurrentWorkLoad = table.Column<int>(type: "int", nullable: false),
                    LastCheckedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LatencyMs = table.Column<int>(type: "int", nullable: true),
                    Error = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HunterServers", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HunterServers");
        }
    }
}
