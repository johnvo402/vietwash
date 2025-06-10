using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20250606012032_Ecommerce_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_order_user_customer_id",
                table: "order");

            migrationBuilder.DropForeignKey(
                name: "fk_voucher_customer_user_customer_id",
                table: "voucher_customer");

            migrationBuilder.DropForeignKey(
                name: "fk_voucher_customer_voucher_voucher_id",
                table: "voucher_customer");

            migrationBuilder.DropPrimaryKey(
                name: "pk_voucher_customer",
                table: "voucher_customer");

            migrationBuilder.RenameTable(
                name: "voucher_customer",
                newName: "voucher_account");

            migrationBuilder.RenameColumn(
                name: "customer_id",
                table: "order",
                newName: "account_id");

            migrationBuilder.RenameIndex(
                name: "ix_order_customer_id",
                table: "order",
                newName: "ix_order_account_id");

            migrationBuilder.RenameColumn(
                name: "customer_id",
                table: "voucher_account",
                newName: "account_id");

            migrationBuilder.RenameIndex(
                name: "ix_voucher_customer_voucher_id",
                table: "voucher_account",
                newName: "ix_voucher_account_voucher_id");

            migrationBuilder.RenameIndex(
                name: "ix_voucher_customer_customer_id",
                table: "voucher_account",
                newName: "ix_voucher_account_account_id");

            migrationBuilder.AlterColumn<int>(
                name: "status",
                table: "supplier",
                type: "integer",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AddColumn<bool>(
                name: "disable",
                table: "supplier",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "image",
                table: "supplier",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "public_id",
                table: "supplier",
                type: "character varying(26)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "version",
                table: "supplier",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<decimal>(
                name: "id",
                table: "repair_history",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddPrimaryKey(
                name: "pk_voucher_account",
                table: "voucher_account",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_order_user_account_id",
                table: "order",
                column: "account_id",
                principalTable: "user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_voucher_account_user_account_id",
                table: "voucher_account",
                column: "account_id",
                principalTable: "user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_voucher_account_voucher_voucher_id",
                table: "voucher_account",
                column: "voucher_id",
                principalTable: "voucher",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_order_user_account_id",
                table: "order");

            migrationBuilder.DropForeignKey(
                name: "fk_voucher_account_user_account_id",
                table: "voucher_account");

            migrationBuilder.DropForeignKey(
                name: "fk_voucher_account_voucher_voucher_id",
                table: "voucher_account");

            migrationBuilder.DropPrimaryKey(
                name: "pk_voucher_account",
                table: "voucher_account");

            migrationBuilder.DropColumn(
                name: "disable",
                table: "supplier");

            migrationBuilder.DropColumn(
                name: "image",
                table: "supplier");

            migrationBuilder.DropColumn(
                name: "public_id",
                table: "supplier");

            migrationBuilder.DropColumn(
                name: "version",
                table: "supplier");

            migrationBuilder.RenameTable(
                name: "voucher_account",
                newName: "voucher_customer");

            migrationBuilder.RenameColumn(
                name: "account_id",
                table: "order",
                newName: "customer_id");

            migrationBuilder.RenameIndex(
                name: "ix_order_account_id",
                table: "order",
                newName: "ix_order_customer_id");

            migrationBuilder.RenameColumn(
                name: "account_id",
                table: "voucher_customer",
                newName: "customer_id");

            migrationBuilder.RenameIndex(
                name: "ix_voucher_account_voucher_id",
                table: "voucher_customer",
                newName: "ix_voucher_customer_voucher_id");

            migrationBuilder.RenameIndex(
                name: "ix_voucher_account_account_id",
                table: "voucher_customer",
                newName: "ix_voucher_customer_customer_id");

            migrationBuilder.AlterColumn<short>(
                name: "status",
                table: "supplier",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<decimal>(
                name: "id",
                table: "repair_history",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)");

            migrationBuilder.AddPrimaryKey(
                name: "pk_voucher_customer",
                table: "voucher_customer",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_order_user_customer_id",
                table: "order",
                column: "customer_id",
                principalTable: "user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_voucher_customer_user_customer_id",
                table: "voucher_customer",
                column: "customer_id",
                principalTable: "user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_voucher_customer_voucher_voucher_id",
                table: "voucher_customer",
                column: "voucher_id",
                principalTable: "voucher",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
