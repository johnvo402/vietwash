using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20250810211802_Ecommerce_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_product_supplying_supplier_supplier_id",
                table: "product_supplying");

            migrationBuilder.DropTable(
                name: "group_service");

            migrationBuilder.DropTable(
                name: "print_template");

            migrationBuilder.DropTable(
                name: "group");

            migrationBuilder.DropColumn(
                name: "type",
                table: "service");

            migrationBuilder.DropColumn(
                name: "expiry_date",
                table: "product_supplying");

            migrationBuilder.DropColumn(
                name: "lot_number",
                table: "product_supplying");

            migrationBuilder.AlterColumn<long>(
                name: "supplier_id",
                table: "product_supplying",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<int>(
                name: "vat",
                table: "order",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "vat_amount",
                table: "order",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "order_equipment",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    order_id = table.Column<long>(type: "bigint", nullable: false),
                    equipment_id = table.Column<long>(type: "bigint", nullable: false),
                    equipment_name = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_equipment", x => x.id);
                    table.ForeignKey(
                        name: "fk_order_equipment_equipment_equipment_id",
                        column: x => x.equipment_id,
                        principalTable: "equipment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_order_equipment_order_order_id",
                        column: x => x.order_id,
                        principalTable: "order",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "service_resource",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    unit_product_id = table.Column<long>(type: "bigint", nullable: false),
                    unit_relation_id = table.Column<long>(type: "bigint", nullable: false),
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    branch_product_id = table.Column<long>(type: "bigint", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_resource", x => x.id);
                    table.ForeignKey(
                        name: "fk_service_resource_branch_product_branch_product_id",
                        column: x => x.branch_product_id,
                        principalTable: "branch_product",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_service_resource_unit_relation_unit_product_id",
                        column: x => x.unit_product_id,
                        principalTable: "unit_relation",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_service_resource_unit_relation_unit_relation_id",
                        column: x => x.unit_relation_id,
                        principalTable: "unit_relation",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_order_equipment_equipment_id",
                table: "order_equipment",
                column: "equipment_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_equipment_id",
                table: "order_equipment",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_order_equipment_order_id",
                table: "order_equipment",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_resource_branch_product_id",
                table: "service_resource",
                column: "branch_product_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_resource_id",
                table: "service_resource",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_service_resource_unit_product_id",
                table: "service_resource",
                column: "unit_product_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_resource_unit_relation_id",
                table: "service_resource",
                column: "unit_relation_id");

            migrationBuilder.AddForeignKey(
                name: "fk_product_supplying_supplier_supplier_id",
                table: "product_supplying",
                column: "supplier_id",
                principalTable: "supplier",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_product_supplying_supplier_supplier_id",
                table: "product_supplying");

            migrationBuilder.DropTable(
                name: "order_equipment");

            migrationBuilder.DropTable(
                name: "service_resource");

            migrationBuilder.DropColumn(
                name: "vat",
                table: "order");

            migrationBuilder.DropColumn(
                name: "vat_amount",
                table: "order");

            migrationBuilder.AddColumn<int>(
                name: "type",
                table: "service",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<long>(
                name: "supplier_id",
                table: "product_supplying",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "expiry_date",
                table: "product_supplying",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "lot_number",
                table: "product_supplying",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "group",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "citext", nullable: true),
                    disable = table.Column<bool>(type: "boolean", nullable: false),
                    name = table.Column<string>(type: "citext", nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    public_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_group", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "print_template",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    html_template = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    public_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_print_template", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "group_service",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    group_id = table.Column<long>(type: "bigint", nullable: false),
                    service_id = table.Column<long>(type: "bigint", nullable: false),
                    unit_relation_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    public_id = table.Column<string>(type: "character varying(26)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_group_service", x => x.id);
                    table.ForeignKey(
                        name: "fk_group_service_group_group_id",
                        column: x => x.group_id,
                        principalTable: "group",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_group_service_service_service_id",
                        column: x => x.service_id,
                        principalTable: "service",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_group_service_unit_relation_unit_relation_id",
                        column: x => x.unit_relation_id,
                        principalTable: "unit_relation",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_group_id",
                table: "group",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_group_service_group_id",
                table: "group_service",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "ix_group_service_service_id",
                table: "group_service",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "ix_group_service_unit_relation_id",
                table: "group_service",
                column: "unit_relation_id");

            migrationBuilder.CreateIndex(
                name: "ix_print_template_id",
                table: "print_template",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_product_supplying_supplier_supplier_id",
                table: "product_supplying",
                column: "supplier_id",
                principalTable: "supplier",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
