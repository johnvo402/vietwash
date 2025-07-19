using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20250718204141_Ecommerce_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_voucher_customer_voucher_voucher_id",
                table: "voucher_customer");

            migrationBuilder.DropTable(
                name: "voucher_customer_group");

            migrationBuilder.DropTable(
                name: "voucher_usage");

            migrationBuilder.AddColumn<bool>(
                name: "is_used",
                table: "voucher_customer",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "fk_voucher_customer_voucher_voucher_id",
                table: "voucher_customer",
                column: "voucher_id",
                principalTable: "voucher",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_voucher_customer_voucher_voucher_id",
                table: "voucher_customer");

            migrationBuilder.DropColumn(
                name: "is_used",
                table: "voucher_customer");

            migrationBuilder.CreateTable(
                name: "voucher_customer_group",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    voucher_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    group = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_voucher_customer_group", x => x.id);
                    table.ForeignKey(
                        name: "fk_voucher_customer_group_voucher_voucher_id",
                        column: x => x.voucher_id,
                        principalTable: "voucher",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "voucher_usage",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    order_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    customer_id = table.Column<long>(type: "bigint", nullable: false),
                    discount_apply = table.Column<decimal>(type: "numeric", nullable: false),
                    public_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    version = table.Column<long>(type: "bigint", nullable: false),
                    voucher_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_voucher_usage", x => x.id);
                    table.ForeignKey(
                        name: "fk_voucher_usage_order_order_id",
                        column: x => x.order_id,
                        principalTable: "order",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_voucher_usage_user_customer_id",
                        column: x => x.customer_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_voucher_usage_voucher_voucher_id",
                        column: x => x.voucher_id,
                        principalTable: "voucher",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_voucher_customer_group_voucher_id_group",
                table: "voucher_customer_group",
                columns: new[] { "voucher_id", "group" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_voucher_usage_customer_id",
                table: "voucher_usage",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_voucher_usage_order_id",
                table: "voucher_usage",
                column: "order_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_voucher_usage_voucher_id",
                table: "voucher_usage",
                column: "voucher_id");

            migrationBuilder.AddForeignKey(
                name: "fk_voucher_customer_voucher_voucher_id",
                table: "voucher_customer",
                column: "voucher_id",
                principalTable: "voucher",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
