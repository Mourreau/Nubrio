using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nubrio.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestsTimestampIdIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_requests_timestamp_utc",
                table: "requests");

            migrationBuilder.CreateIndex(
                name: "ix_requests_timestamp_utc_id",
                table: "requests",
                columns: new[] { "timestamp_utc", "id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_requests_timestamp_utc_id",
                table: "requests");

            migrationBuilder.CreateIndex(
                name: "ix_requests_timestamp_utc",
                table: "requests",
                column: "timestamp_utc");
        }
    }
}
