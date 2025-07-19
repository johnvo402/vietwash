using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20250719124620_Ecommerce_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "performed_date",
                table: "equipment_activity");

            migrationBuilder.DropColumn(
                name: "version",
                table: "equipment_activity");

            migrationBuilder.AddColumn<string>(
                name: "supervisor_name",
                table: "equipment_activity",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "supervisor_name",
                table: "equipment_activity");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "performed_date",
                table: "equipment_activity",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "version",
                table: "equipment_activity",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
