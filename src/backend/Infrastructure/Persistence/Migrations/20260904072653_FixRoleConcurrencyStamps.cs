using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixRoleConcurrencyStamps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "roles",
                keyColumn: "id",
                keyValue: 1L,
                column: "concurrency_stamp",
                value: "253a6d29-edd9-4a7d-8a1a-e8a9debf68c3");

            migrationBuilder.UpdateData(
                table: "roles",
                keyColumn: "id",
                keyValue: 2L,
                column: "concurrency_stamp",
                value: "d722a810-9a7a-4cfa-a572-8b7f94d6802b");

            migrationBuilder.UpdateData(
                table: "roles",
                keyColumn: "id",
                keyValue: 3L,
                column: "concurrency_stamp",
                value: "479ec9d3-28a7-4d6e-abab-16e1121b34dd");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "roles",
                keyColumn: "id",
                keyValue: 1L,
                column: "concurrency_stamp",
                value: "aee9cb30-b09c-423b-9373-53c01c52144a");

            migrationBuilder.UpdateData(
                table: "roles",
                keyColumn: "id",
                keyValue: 2L,
                column: "concurrency_stamp",
                value: "12756d8b-eaa6-47eb-95b0-939125534145");

            migrationBuilder.UpdateData(
                table: "roles",
                keyColumn: "id",
                keyValue: 3L,
                column: "concurrency_stamp",
                value: "a60104ee-c8eb-4841-a480-e2a4d4d7bba7");
        }
    }
}
