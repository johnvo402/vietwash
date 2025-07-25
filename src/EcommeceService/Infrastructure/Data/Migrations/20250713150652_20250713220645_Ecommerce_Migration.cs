using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20250713220645_Ecommerce_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_equipment_supplying_unit_relation_unit_relation_id",
                table: "equipment_supplying"
            );
            migrationBuilder.DropForeignKey(
                name: "fk_repair_detail_repair_history_repair_history_id",
                table: "repair_detail"
            );

            migrationBuilder.DropIndex(
                name: "ix_equipment_supplying_unit_relation_id",
                table: "equipment_supplying"
            );

            migrationBuilder.DropColumn(name: "branch_id", table: "inventory_request");

            migrationBuilder.DropColumn(name: "from_warehouse_id", table: "inventory_document");

            migrationBuilder.DropColumn(name: "to_warehouse_id", table: "inventory_document");

            migrationBuilder.DropColumn(name: "unit_relation_id", table: "equipment_supplying");

            migrationBuilder.RenameColumn(
                name: "to_warehouse_id",
                table: "inventory_request",
                newName: "to_branch_id"
            );

            migrationBuilder.RenameColumn(
                name: "from_warehouse_id",
                table: "inventory_request",
                newName: "from_branch_id"
            );

            migrationBuilder.AddColumn<long>(
                name: "unit_id",
                table: "unit_relation",
                type: "bigint",
                nullable: true
            );

            migrationBuilder.AlterColumn<long>(
                name: "id",
                table: "repair_history",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric"
            );

            migrationBuilder.AlterColumn<long>(
                name: "repair_history_id",
                table: "repair_detail",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)"
            );

            migrationBuilder.CreateIndex(
                name: "ix_unit_relation_unit_id",
                table: "unit_relation",
                column: "unit_id"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_unit_relation_unit_unit_id",
                table: "unit_relation",
                column: "unit_id",
                principalTable: "unit",
                principalColumn: "id"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_repair_detail_repair_history_repair_history_id",
                table: "repair_detail",
                column: "repair_history_id",
                principalTable: "repair_history",
                principalColumn: "id"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_unit_relation_unit_unit_id",
                table: "unit_relation"
            );

            migrationBuilder.DropIndex(name: "ix_unit_relation_unit_id", table: "unit_relation");

            migrationBuilder.DropColumn(name: "unit_id", table: "unit_relation");

            migrationBuilder.RenameColumn(
                name: "to_branch_id",
                table: "inventory_request",
                newName: "to_warehouse_id"
            );

            migrationBuilder.RenameColumn(
                name: "from_branch_id",
                table: "inventory_request",
                newName: "from_warehouse_id"
            );

            migrationBuilder.AlterColumn<decimal>(
                name: "id",
                table: "repair_history",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint"
            );

            migrationBuilder.AlterColumn<decimal>(
                name: "repair_history_id",
                table: "repair_detail",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint"
            );

            migrationBuilder.AddColumn<long>(
                name: "branch_id",
                table: "inventory_request",
                type: "bigint",
                nullable: true
            );

            migrationBuilder.AddColumn<long>(
                name: "from_warehouse_id",
                table: "inventory_document",
                type: "bigint",
                nullable: true
            );

            migrationBuilder.AddColumn<long>(
                name: "to_warehouse_id",
                table: "inventory_document",
                type: "bigint",
                nullable: true
            );

            migrationBuilder.AddColumn<long>(
                name: "unit_relation_id",
                table: "equipment_supplying",
                type: "bigint",
                nullable: false,
                defaultValue: 0L
            );

            migrationBuilder.CreateIndex(
                name: "ix_equipment_supplying_unit_relation_id",
                table: "equipment_supplying",
                column: "unit_relation_id"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_equipment_supplying_unit_relation_unit_relation_id",
                table: "equipment_supplying",
                column: "unit_relation_id",
                principalTable: "unit_relation",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade
            );
        }
    }
}
