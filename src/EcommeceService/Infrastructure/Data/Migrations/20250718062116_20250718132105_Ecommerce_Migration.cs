using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20250718132105_Ecommerce_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_voucher_customer_user_customer_id",
                table: "voucher_customer"
            );

            migrationBuilder.DropForeignKey(
                name: "fk_voucher_customer_voucher_voucher_id",
                table: "voucher_customer"
            );

            migrationBuilder.DropColumn(name: "is_used", table: "voucher_customer");

            migrationBuilder.DropColumn(name: "customer_groups", table: "voucher");

            migrationBuilder.CreateTable(
                name: "voucher_customer_group",
                columns: table => new
                {
                    voucher_id = table.Column<long>(type: "bigint", nullable: false),
                    group = table.Column<short>(type: "smallint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "pk_voucher_customer_group",
                        x => new { x.voucher_id, x.group }
                    );
                    table.ForeignKey(
                        name: "fk_voucher_customer_group_voucher_voucher_id",
                        column: x => x.voucher_id,
                        principalTable: "voucher",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.AddForeignKey(
                name: "fk_voucher_customer_user_customer_id",
                table: "voucher_customer",
                column: "customer_id",
                principalTable: "user",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "fk_voucher_customer_voucher_voucher_id",
                table: "voucher_customer",
                column: "voucher_id",
                principalTable: "voucher",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_voucher_customer_user_customer_id",
                table: "voucher_customer"
            );

            migrationBuilder.DropForeignKey(
                name: "fk_voucher_customer_voucher_voucher_id",
                table: "voucher_customer"
            );

            migrationBuilder.DropTable(name: "voucher_customer_group");

            migrationBuilder.AddColumn<bool>(
                name: "is_used",
                table: "voucher_customer",
                type: "boolean",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<short[]>(
                name: "customer_groups",
                table: "voucher",
                type: "smallint[]",
                nullable: false,
                defaultValue: new short[0]
            );

            migrationBuilder.AddForeignKey(
                name: "fk_voucher_customer_user_customer_id",
                table: "voucher_customer",
                column: "customer_id",
                principalTable: "user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "fk_voucher_customer_voucher_voucher_id",
                table: "voucher_customer",
                column: "voucher_id",
                principalTable: "voucher",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade
            );
        }
    }
}
