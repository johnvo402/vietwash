using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20250618144151_Ecommerce_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "email_enabled", table: "user");

            migrationBuilder.DropColumn(name: "phone_enabled", table: "user");

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "user",
                type: "citext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "citext"
            );

            migrationBuilder.AddColumn<short>(
                name: "customer_group",
                table: "user",
                type: "smallint",
                nullable: true
            );

            migrationBuilder.AlterColumn<decimal>(
                name: "id",
                table: "repair_history",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric"
            );

            migrationBuilder.CreateTable(
                name: "get_revenue_statistic",
                columns: table => new
                {
                    revenue_date = table.Column<DateOnly>(type: "date", nullable: false),
                    total_revenue = table.Column<decimal>(type: "numeric", nullable: false),
                },
                constraints: table => { }
            );

            migrationBuilder.CreateTable(
                name: "order_summary_result",
                columns: table => new
                {
                    order_id = table.Column<long>(type: "bigint", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    branch_id = table.Column<long>(type: "bigint", nullable: false),
                    customer_id = table.Column<long>(type: "bigint", nullable: false),
                    order_item_count = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    order_date = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                },
                constraints: table => { }
            );
            var basePath = AppContext.BaseDirectory;

            var fnPath = Path.Combine(
                basePath,
                "Data",
                "Migrations",
                "get_order_summary_v2",
                "up.sql"
            );
            var sql = File.ReadAllText(fnPath);
            migrationBuilder.Sql(sql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "get_revenue_statistic");

            migrationBuilder.DropTable(name: "order_summary_result");

            migrationBuilder.DropColumn(name: "customer_group", table: "user");

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "user",
                type: "citext",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "citext",
                oldNullable: true
            );

            migrationBuilder.AddColumn<bool>(
                name: "email_enabled",
                table: "user",
                type: "boolean",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<bool>(
                name: "phone_enabled",
                table: "user",
                type: "boolean",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AlterColumn<decimal>(
                name: "id",
                table: "repair_history",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)"
            );
            var basePath = AppContext.BaseDirectory;

            var fnPath = Path.Combine(
                basePath,
                "Data",
                "Migrations",
                "get_order_summary_v2",
                "down.sql"
            );
            var sql = File.ReadAllText(fnPath);
            migrationBuilder.Sql(sql);
        }
    }
}
