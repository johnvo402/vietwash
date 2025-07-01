using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20250630163549_Ecommerce_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM service;");
            migrationBuilder.DropColumn(name: "category_id", table: "service");
            migrationBuilder.AddColumn<long>(
                name: "category_id",
                table: "service",
                type: "bigint",
                nullable: false
            );

            migrationBuilder.AlterColumn<decimal>(
                name: "id",
                table: "repair_history",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric"
            );

            migrationBuilder.Sql("DELETE FROM category;");

            migrationBuilder.DropColumn(name: "id", table: "category");

            migrationBuilder.AddColumn<long>(
                name: "id",
                table: "category",
                type: "bigint",
                nullable: false
            );

            // Thêm primary key
            migrationBuilder.AddPrimaryKey(
                name: "PK_category", // Tên khóa chính, có thể tùy chỉnh
                table: "category",
                column: "id"
            );

            // Thêm index cho id (thường index tự động tạo khi là primary key, nhưng nếu bạn muốn index riêng có thể thêm)
            migrationBuilder.CreateIndex(name: "IX_category_id", table: "category", column: "id");

            migrationBuilder.AddColumn<string>(
                name: "code",
                table: "category",
                type: "text",
                nullable: false,
                defaultValue: ""
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "code", table: "category");

            migrationBuilder.Sql("DELETE FROM service;");
            migrationBuilder.DropColumn(name: "category_id", table: "service");
            migrationBuilder.AddColumn<string>(
                name: "category_id",
                table: "service",
                type: "text",
                nullable: false
            );

            migrationBuilder.AlterColumn<decimal>(
                name: "id",
                table: "repair_history",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)"
            );

            migrationBuilder.Sql("DELETE FROM category;");

            migrationBuilder.DropColumn(name: "id", table: "category");

            migrationBuilder.AddColumn<string>(
                name: "id",
                table: "category",
                type: "text",
                nullable: false
            );

            // Thêm primary key
            migrationBuilder.AddPrimaryKey(
                name: "PK_category", // Tên khóa chính, có thể tùy chỉnh
                table: "category",
                column: "id"
            );

            // Thêm index cho id (thường index tự động tạo khi là primary key, nhưng nếu bạn muốn index riêng có thể thêm)
            migrationBuilder.CreateIndex(name: "IX_category_id", table: "category", column: "id");
        }
    }
}
