using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20250702211355_Ecommerce_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_unit_relation_service_reference_id",
                table: "unit_relation"
            );

            migrationBuilder.DropIndex(
                name: "ix_unit_relation_reference_id",
                table: "unit_relation"
            );

            migrationBuilder.DropColumn(name: "reference_id", table: "unit_relation");

            migrationBuilder.AddColumn<long>(
                name: "branch_product_id",
                table: "unit_relation",
                type: "bigint",
                nullable: true
            );

            migrationBuilder.AddColumn<long>(
                name: "service_id",
                table: "unit_relation",
                type: "bigint",
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
                name: "branch_product",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    branch_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    sku = table.Column<string>(type: "text", nullable: true),
                    barcode = table.Column<string>(type: "text", nullable: true),
                    image = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    disable = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_branch_product", x => x.id);
                }
            );

            migrationBuilder.CreateIndex(
                name: "ix_unit_relation_branch_product_id",
                table: "unit_relation",
                column: "branch_product_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_unit_relation_service_id",
                table: "unit_relation",
                column: "service_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_branch_product_id",
                table: "branch_product",
                column: "id"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_unit_relation_branch_product_branch_product_id",
                table: "unit_relation",
                column: "branch_product_id",
                principalTable: "branch_product",
                principalColumn: "id"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_unit_relation_service_service_id",
                table: "unit_relation",
                column: "service_id",
                principalTable: "service",
                principalColumn: "id"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_unit_relation_branch_product_branch_product_id",
                table: "unit_relation"
            );

            migrationBuilder.DropForeignKey(
                name: "fk_unit_relation_service_service_id",
                table: "unit_relation"
            );

            migrationBuilder.DropTable(name: "branch_product");

            migrationBuilder.DropIndex(
                name: "ix_unit_relation_branch_product_id",
                table: "unit_relation"
            );

            migrationBuilder.DropIndex(name: "ix_unit_relation_service_id", table: "unit_relation");

            migrationBuilder.DropColumn(name: "branch_product_id", table: "unit_relation");

            migrationBuilder.DropColumn(name: "service_id", table: "unit_relation");

            migrationBuilder.AddColumn<long>(
                name: "reference_id",
                table: "unit_relation",
                type: "bigint",
                nullable: false,
                defaultValue: 0L
            );

            migrationBuilder.AlterColumn<decimal>(
                name: "id",
                table: "repair_history",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)"
            );

            migrationBuilder.CreateIndex(
                name: "ix_unit_relation_reference_id",
                table: "unit_relation",
                column: "reference_id"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_unit_relation_service_reference_id",
                table: "unit_relation",
                column: "reference_id",
                principalTable: "service",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade
            );
        }
    }
}
