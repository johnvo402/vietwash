using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20250715084003_Ecommerce_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "maintenance_detail");

            migrationBuilder.DropTable(
                name: "repair_detail");

            migrationBuilder.DropTable(
                name: "maintenance_history");

            migrationBuilder.DropTable(
                name: "repair_history");

            migrationBuilder.CreateTable(
                name: "equipment_activity",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    equipment_id = table.Column<long>(type: "bigint", nullable: false),
                    branch_id = table.Column<long>(type: "bigint", nullable: false),
                    staff_id = table.Column<long>(type: "bigint", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    reported_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    scheduled_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    labor_cost = table.Column<decimal>(type: "numeric", nullable: false),
                    total_cost = table.Column<decimal>(type: "numeric", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    received_by = table.Column<string>(type: "text", nullable: true),
                    supervisor_code = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    public_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_equipment_activity", x => x.id);
                    table.ForeignKey(
                        name: "fk_equipment_activity_equipment_equipment_id",
                        column: x => x.equipment_id,
                        principalTable: "equipment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_equipment_activity_user_staff_id",
                        column: x => x.staff_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "equipment_activity_detail",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    part_name = table.Column<string>(type: "text", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    equipment_activity_id = table.Column<long>(type: "bigint", nullable: false),
                    public_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_equipment_activity_detail", x => x.id);
                    table.ForeignKey(
                        name: "fk_equipment_activity_detail_equipment_activity_equipment_acti",
                        column: x => x.equipment_activity_id,
                        principalTable: "equipment_activity",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_equipment_activity_equipment_id",
                table: "equipment_activity",
                column: "equipment_id");

            migrationBuilder.CreateIndex(
                name: "ix_equipment_activity_staff_id",
                table: "equipment_activity",
                column: "staff_id");

            migrationBuilder.CreateIndex(
                name: "ix_equipment_activity_detail_equipment_activity_id",
                table: "equipment_activity_detail",
                column: "equipment_activity_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "equipment_activity_detail");

            migrationBuilder.DropTable(
                name: "equipment_activity");

            migrationBuilder.CreateTable(
                name: "maintenance_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    equipment_id = table.Column<long>(type: "bigint", nullable: false),
                    branch_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    maintenance_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    next_maintenance_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    public_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    supervisor = table.Column<string>(type: "text", nullable: false),
                    total = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_maintenance_history", x => x.id);
                    table.ForeignKey(
                        name: "fk_maintenance_history_equipment_equipment_id",
                        column: x => x.equipment_id,
                        principalTable: "equipment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "repair_history",
                columns: table => new
                {
                    id = table.Column<decimal>(type: "numeric", nullable: false),
                    equipment_id = table.Column<long>(type: "bigint", nullable: false),
                    branch_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    public_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    received_by = table.Column<string>(type: "text", nullable: false),
                    repair_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    total = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_repair_history", x => x.id);
                    table.ForeignKey(
                        name: "fk_repair_history_equipment_equipment_id",
                        column: x => x.equipment_id,
                        principalTable: "equipment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintenance_detail",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    maintenance_history_id = table.Column<long>(type: "bigint", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    part_name = table.Column<string>(type: "text", nullable: false),
                    public_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_maintenance_detail", x => x.id);
                    table.ForeignKey(
                        name: "fk_maintenance_detail_maintenance_history_maintenance_history_",
                        column: x => x.maintenance_history_id,
                        principalTable: "maintenance_history",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "repair_detail",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    repair_history_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    part_name = table.Column<string>(type: "text", nullable: false),
                    public_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_repair_detail", x => x.id);
                    table.ForeignKey(
                        name: "fk_repair_detail_repair_history_repair_history_id",
                        column: x => x.repair_history_id,
                        principalTable: "repair_history",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_detail_maintenance_history_id",
                table: "maintenance_detail",
                column: "maintenance_history_id");

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_history_equipment_id",
                table: "maintenance_history",
                column: "equipment_id");

            migrationBuilder.CreateIndex(
                name: "ix_repair_detail_repair_history_id",
                table: "repair_detail",
                column: "repair_history_id");

            migrationBuilder.CreateIndex(
                name: "ix_repair_history_equipment_id",
                table: "repair_history",
                column: "equipment_id");
        }
    }
}
