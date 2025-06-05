using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20250606013317_Ecommerce_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "fk_order_user_account_id", table: "order");

            migrationBuilder.DropTable(name: "voucher_account");

            migrationBuilder.RenameColumn(
                name: "account_id",
                table: "order",
                newName: "customer_id"
            );

            migrationBuilder.RenameIndex(
                name: "ix_order_account_id",
                table: "order",
                newName: "ix_order_customer_id"
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
                name: "voucher_customer",
                columns: table => new
                {
                    id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    customer_id = table.Column<long>(type: "bigint", nullable: false),
                    voucher_id = table.Column<long>(type: "bigint", nullable: false),
                    is_used = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_voucher_customer", x => x.id);
                    table.ForeignKey(
                        name: "fk_voucher_customer_user_customer_id",
                        column: x => x.customer_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "fk_voucher_customer_voucher_voucher_id",
                        column: x => x.voucher_id,
                        principalTable: "voucher",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "ix_voucher_customer_customer_id",
                table: "voucher_customer",
                column: "customer_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_voucher_customer_voucher_id",
                table: "voucher_customer",
                column: "voucher_id"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_order_user_customer_id",
                table: "order",
                column: "customer_id",
                principalTable: "user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "fk_order_user_customer_id", table: "order");

            migrationBuilder.DropTable(name: "voucher_customer");

            migrationBuilder.RenameColumn(
                name: "customer_id",
                table: "order",
                newName: "account_id"
            );

            migrationBuilder.RenameIndex(
                name: "ix_order_customer_id",
                table: "order",
                newName: "ix_order_account_id"
            );

            migrationBuilder.AlterColumn<decimal>(
                name: "id",
                table: "repair_history",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)"
            );

            migrationBuilder.CreateTable(
                name: "voucher_account",
                columns: table => new
                {
                    id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    account_id = table.Column<long>(type: "bigint", nullable: false),
                    voucher_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    is_used = table.Column<bool>(type: "boolean", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_voucher_account", x => x.id);
                    table.ForeignKey(
                        name: "fk_voucher_account_user_account_id",
                        column: x => x.account_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "fk_voucher_account_voucher_voucher_id",
                        column: x => x.voucher_id,
                        principalTable: "voucher",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "ix_voucher_account_account_id",
                table: "voucher_account",
                column: "account_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_voucher_account_voucher_id",
                table: "voucher_account",
                column: "voucher_id"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_order_user_account_id",
                table: "order",
                column: "account_id",
                principalTable: "user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade
            );
        }
    }
}
