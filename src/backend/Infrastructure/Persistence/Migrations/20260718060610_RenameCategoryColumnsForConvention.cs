using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameCategoryColumnsForConvention : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "name",
                table: "categories",
                newName: "category_name");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "categories",
                newName: "category_description");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "categories",
                newName: "category_id");

            migrationBuilder.RenameIndex(
                name: "IX_categories_name",
                table: "categories",
                newName: "IX_categories_category_name");

            migrationBuilder.UpdateData(
                table: "roles",
                keyColumn: "id",
                keyValue: 1L,
                column: "concurrency_stamp",
                value: "e6b2bb3a-d7e3-4fd9-b9f4-d9c6adec1dd0");

            migrationBuilder.UpdateData(
                table: "roles",
                keyColumn: "id",
                keyValue: 2L,
                column: "concurrency_stamp",
                value: "eafa629d-8124-4ca4-88b2-741f833343cc");

            migrationBuilder.UpdateData(
                table: "roles",
                keyColumn: "id",
                keyValue: 3L,
                column: "concurrency_stamp",
                value: "c3d9067e-65c0-4ed8-b904-f97d61f20bdb");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "category_name",
                table: "categories",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "category_description",
                table: "categories",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "category_id",
                table: "categories",
                newName: "id");

            migrationBuilder.RenameIndex(
                name: "IX_categories_category_name",
                table: "categories",
                newName: "IX_categories_name");

            migrationBuilder.UpdateData(
                table: "roles",
                keyColumn: "id",
                keyValue: 1L,
                column: "concurrency_stamp",
                value: "feb86115-2b7d-44bb-8439-d04b92657548");

            migrationBuilder.UpdateData(
                table: "roles",
                keyColumn: "id",
                keyValue: 2L,
                column: "concurrency_stamp",
                value: "7076df0c-8535-4765-a130-49f002b0e52a");

            migrationBuilder.UpdateData(
                table: "roles",
                keyColumn: "id",
                keyValue: 3L,
                column: "concurrency_stamp",
                value: "8b0e6c3f-1c17-460d-89d3-9540a4a5f3e6");
        }
    }
}
