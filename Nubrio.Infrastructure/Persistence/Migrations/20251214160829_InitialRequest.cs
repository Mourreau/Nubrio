using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nubrio.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    timestamp_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    endpoint = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: true),
                    cache_hit = table.Column<bool>(type: "boolean", nullable: false),
                    status_code = table.Column<int>(type: "integer", nullable: false),
                    latency_ms = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_requests", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_requests_timestamp_utc",
                table: "requests",
                column: "timestamp_utc");

            migrationBuilder.CreateIndex(
                name: "ix_requests_timestamp_utc_city",
                table: "requests",
                columns: new[] { "timestamp_utc", "city" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "requests");
        }
    }
}
