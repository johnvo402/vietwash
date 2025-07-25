using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20250718134746_Ecommerce_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_voucher_customer_group",
                table: "voucher_customer_group"
            );

            migrationBuilder.AddColumn<long>(
                name: "id",
                table: "voucher_customer_group",
                type: "bigint",
                nullable: false,
                defaultValue: 0L
            );

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_at",
                table: "voucher_customer_group",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(
                    new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                    new TimeSpan(0, 0, 0, 0, 0)
                )
            );

            migrationBuilder.AddPrimaryKey(
                name: "pk_voucher_customer_group",
                table: "voucher_customer_group",
                column: "id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_voucher_customer_group_voucher_id_group",
                table: "voucher_customer_group",
                columns: new[] { "voucher_id", "group" },
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_voucher_customer_group",
                table: "voucher_customer_group"
            );

            migrationBuilder.DropIndex(
                name: "ix_voucher_customer_group_voucher_id_group",
                table: "voucher_customer_group"
            );

            migrationBuilder.DropColumn(name: "id", table: "voucher_customer_group");

            migrationBuilder.DropColumn(name: "created_at", table: "voucher_customer_group");

            migrationBuilder.AddPrimaryKey(
                name: "pk_voucher_customer_group",
                table: "voucher_customer_group",
                columns: new[] { "voucher_id", "group" }
            );
        }
    }
}
