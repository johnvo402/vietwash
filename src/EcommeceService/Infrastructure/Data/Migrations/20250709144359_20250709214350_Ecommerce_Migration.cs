using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20250709214350_Ecommerce_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short[]>(
                name: "customer_groups",
                table: "voucher",
                type: "smallint[]",
                nullable: false,
                defaultValue: new short[0]
            );

            migrationBuilder.AddColumn<int>(
                name: "total_quantity",
                table: "voucher",
                type: "integer",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AddColumn<int>(
                name: "used_quantity",
                table: "voucher",
                type: "integer",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.CreateTable(
                name: "voucher_usage",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    voucher_id = table.Column<long>(type: "bigint", nullable: false),
                    customer_id = table.Column<long>(type: "bigint", nullable: false),
                    order_id = table.Column<long>(type: "bigint", nullable: false),
                    discount_apply = table.Column<decimal>(type: "numeric", nullable: false),
                    public_id = table.Column<string>(
                        type: "character varying(26)",
                        nullable: false
                    ),
                    created_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    version = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_voucher_usage", x => x.id);
                    table.ForeignKey(
                        name: "fk_voucher_usage_order_order_id",
                        column: x => x.order_id,
                        principalTable: "order",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "fk_voucher_usage_user_customer_id",
                        column: x => x.customer_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "fk_voucher_usage_voucher_voucher_id",
                        column: x => x.voucher_id,
                        principalTable: "voucher",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "ix_voucher_usage_customer_id",
                table: "voucher_usage",
                column: "customer_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_voucher_usage_order_id",
                table: "voucher_usage",
                column: "order_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_voucher_usage_voucher_id",
                table: "voucher_usage",
                column: "voucher_id"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "voucher_usage");

            migrationBuilder.DropColumn(name: "customer_groups", table: "voucher");

            migrationBuilder.DropColumn(name: "total_quantity", table: "voucher");

            migrationBuilder.DropColumn(name: "used_quantity", table: "voucher");
        }
    }
}
