using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderCancellationAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "cancellation_reason",
                table: "order",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true
            );

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "cancelled_at",
                table: "order",
                type: "timestamp with time zone",
                nullable: true
            );

            migrationBuilder.AddColumn<long>(
                name: "cancelled_by",
                table: "order",
                type: "bigint",
                nullable: true
            );

            string sql = File.ReadAllText(
                Path.Combine(
                    AppContext.BaseDirectory,
                    "Data",
                    "Migrations",
                    "get_customer_revenue_report_v2",
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
                    "get_customer_revenue_report",
                    "up.sql"
                )
            );
            migrationBuilder.Sql(sql);

            migrationBuilder.DropColumn(name: "cancellation_reason", table: "order");

            migrationBuilder.DropColumn(name: "cancelled_at", table: "order");

            migrationBuilder.DropColumn(name: "cancelled_by", table: "order");
        }
    }
}
