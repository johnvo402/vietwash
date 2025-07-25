using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20250725202804_Ecommerce_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inventory_relation");

            migrationBuilder.DropTable(
                name: "inventory_invoice");

            migrationBuilder.DropColumn(
                name: "receipt",
                table: "order");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "order_date",
                table: "order",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "order_date",
                table: "order",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "receipt",
                table: "order",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "inventory_invoice",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    public_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    supplier_id = table.Column<long>(type: "bigint", nullable: true),
                    transaction_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inventory_invoice", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "inventory_relation",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    inventory_document_id = table.Column<long>(type: "bigint", nullable: true),
                    inventory_invoice_id = table.Column<long>(type: "bigint", nullable: true),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inventory_relation", x => x.id);
                    table.ForeignKey(
                        name: "fk_inventory_relation_inventory_document_inventory_document_id",
                        column: x => x.inventory_document_id,
                        principalTable: "inventory_document",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_inventory_relation_inventory_invoice_inventory_invoice_id",
                        column: x => x.inventory_invoice_id,
                        principalTable: "inventory_invoice",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_inventory_invoice_id",
                table: "inventory_invoice",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_relation_id",
                table: "inventory_relation",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_relation_inventory_document_id",
                table: "inventory_relation",
                column: "inventory_document_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_relation_inventory_invoice_id",
                table: "inventory_relation",
                column: "inventory_invoice_id");
        }
    }
}
