using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class CorrectCustomerRevenueReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            string sql = File.ReadAllText(
                Path.Combine(
                    AppContext.BaseDirectory,
                    "Data",
                    "Migrations",
                    "get_customer_revenue_report",
                    "up.sql"
                )
            );
            migrationBuilder.Sql(sql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            string sql = File.ReadAllText(
                Path.Combine(
                    AppContext.BaseDirectory,
                    "Data",
                    "Migrations",
                    "get_customer_revenue_report_v1",
                    "up.sql"
                )
            );
            migrationBuilder.Sql(sql);
        }
    }
}
