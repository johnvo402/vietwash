using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20250622130221_Ecommerce_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "customer_revenue_result",
                columns: table => new
                {
                    customer_id = table.Column<long>(type: "bigint", nullable: true),
                    customer_code = table.Column<string>(type: "text", nullable: true),
                    phone_number = table.Column<string>(type: "text", nullable: true),
                    display_name = table.Column<string>(type: "text", nullable: true),
                    avt_url = table.Column<string>(type: "text", nullable: true),
                    revenue = table.Column<decimal>(type: "numeric", nullable: false),
                    cancel_value = table.Column<decimal>(type: "numeric", nullable: false),
                    net_revenue = table.Column<decimal>(type: "numeric", nullable: false),
                    order_sale_quantity = table.Column<int>(type: "integer", nullable: false),
                    order_cancel_quantity = table.Column<int>(type: "integer", nullable: false),
                },
                constraints: table => { }
            );
            var basePath = AppContext.BaseDirectory;
            var fnPath = Path.Combine(
                basePath,
                "Data",
                "Migrations",
                "get_customer_revenue_report",
                "up.sql"
            );
            var sql = File.ReadAllText(fnPath);
            migrationBuilder.Sql(sql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var basePath = AppContext.BaseDirectory;

            migrationBuilder.DropTable(name: "customer_revenue_result");

            var fnPath = Path.Combine(
                basePath,
                "Data",
                "Migrations",
                "get_customer_revenue_report",
                "down.sql"
            );
            var sql = File.ReadAllText(fnPath);
            migrationBuilder.Sql(sql);
        }
    }
}
