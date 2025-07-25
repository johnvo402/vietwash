using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20250725203106_Finance_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "e_invoice",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    invoice_symbol = table.Column<string>(type: "text", nullable: false),
                    invoice_number = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_id = table.Column<long>(type: "bigint", nullable: false),
                    order_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    lookup_code = table.Column<string>(type: "text", nullable: false),
                    org_name = table.Column<string>(type: "text", nullable: false),
                    org_tax_code = table.Column<string>(type: "text", nullable: false),
                    org_address = table.Column<string>(type: "text", nullable: false),
                    org_phone = table.Column<string>(type: "text", nullable: false),
                    org_logo = table.Column<string>(type: "text", nullable: true),
                    org_stamp = table.Column<string>(type: "text", nullable: true),
                    customer_name = table.Column<string>(type: "text", nullable: false),
                    customer_email = table.Column<string>(type: "text", nullable: true),
                    customer_phone = table.Column<string>(type: "text", nullable: true),
                    customer_tax_code = table.Column<string>(type: "text", nullable: true),
                    total = table.Column<decimal>(type: "numeric", nullable: false),
                    vat_percent = table.Column<int>(type: "integer", nullable: false),
                    tax_total = table.Column<decimal>(type: "numeric", nullable: false),
                    total_with_tax = table.Column<decimal>(type: "numeric", nullable: false),
                    qr_code_url = table.Column<string>(type: "text", nullable: true),
                    pdf_url = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    public_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_e_invoice", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "e_invoice_item",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    e_invoice_id = table.Column<long>(type: "bigint", nullable: false),
                    service_name = table.Column<string>(type: "text", nullable: false),
                    unit_relation_name = table.Column<string>(type: "text", nullable: true),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric", nullable: false),
                    total_price = table.Column<decimal>(type: "numeric", nullable: false),
                    public_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_e_invoice_item", x => x.id);
                    table.ForeignKey(
                        name: "fk_e_invoice_item_e_invoice_e_invoice_id",
                        column: x => x.e_invoice_id,
                        principalTable: "e_invoice",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_e_invoice_id",
                table: "e_invoice",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_e_invoice_item_e_invoice_id",
                table: "e_invoice_item",
                column: "e_invoice_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "e_invoice_item");

            migrationBuilder.DropTable(
                name: "e_invoice");
        }
    }
}
