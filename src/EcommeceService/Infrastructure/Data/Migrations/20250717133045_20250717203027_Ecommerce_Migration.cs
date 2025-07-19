using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20250717203027_Ecommerce_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "reported_date",
                table: "equipment_activity");

            migrationBuilder.DropColumn(
                name: "status",
                table: "equipment_activity");

            migrationBuilder.RenameColumn(
                name: "scheduled_date",
                table: "equipment_activity",
                newName: "performed_date");

            migrationBuilder.RenameColumn(
                name: "last_maintenance_date",
                table: "equipment",
                newName: "last_maintenance_or_repair_date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "equipment_activity_detail",
                newName: "repair_detail");

            migrationBuilder.RenameColumn(
                name: "performed_date",
                table: "equipment_activity",
                newName: "scheduled_date");

            migrationBuilder.RenameColumn(
                name: "last_maintenance_or_repair_date",
                table: "equipment",
                newName: "last_maintenance_date");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "reported_date",
                table: "equipment_activity",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "equipment_activity",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "repair_history_id",
                table: "repair_detail",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "repair_history",
                columns: table => new
                {
                    id = table.Column<decimal>(type: "numeric", nullable: false),
                    branch_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    equipment_id = table.Column<long>(type: "bigint", nullable: false),
                    public_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    received_by = table.Column<string>(type: "text", nullable: false),
                    repair_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    total = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_repair_history", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_repair_history_equipment_id",
                table: "repair_history",
                column: "equipment_id");
        }
    }
}
