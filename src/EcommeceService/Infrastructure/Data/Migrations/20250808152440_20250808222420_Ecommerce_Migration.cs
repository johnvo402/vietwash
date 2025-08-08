using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20250808222420_Ecommerce_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "pdf_url", table: "inventory_document");

            migrationBuilder.CreateTable(
                name: "inventory_supplier_receipt",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    inventory_document_id = table.Column<long>(type: "bigint", nullable: false),
                    supplier_id = table.Column<long>(type: "bigint", nullable: false),
                    pdf_url = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inventory_supplier_receipt", x => x.id);
                    table.ForeignKey(
                        name: "fk_inventory_supplier_receipt_inventory_document_inventory_doc",
                        column: x => x.inventory_document_id,
                        principalTable: "inventory_document",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "fk_inventory_supplier_receipt_supplier_supplier_id",
                        column: x => x.supplier_id,
                        principalTable: "supplier",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "ix_inventory_supplier_receipt_inventory_document_id",
                table: "inventory_supplier_receipt",
                column: "inventory_document_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_inventory_supplier_receipt_supplier_id",
                table: "inventory_supplier_receipt",
                column: "supplier_id"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "inventory_supplier_receipt");

            migrationBuilder.AddColumn<string>(
                name: "pdf_url",
                table: "inventory_document",
                type: "text",
                nullable: true
            );
        }
    }
}
