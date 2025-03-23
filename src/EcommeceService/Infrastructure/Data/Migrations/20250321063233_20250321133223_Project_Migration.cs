using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20250321133223_Project_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_fund_payment_method_payment_method_id",
                table: "fund"
            );
            migrationBuilder.DropForeignKey(
                name: "fk_order_payment_method_payment_method_id",
                table: "order"
            );
            migrationBuilder.DropForeignKey(
                name: "fk_order_payment_payment_method_payment_method_id",
                table: "order_payment"
            );

            migrationBuilder.DropTable(name: "payment_method");

            migrationBuilder.DropColumn(name: "payment_method_id", table: "order");

            migrationBuilder.RenameColumn(
                name: "payment_method_id",
                table: "fund",
                newName: "payment_method"
            );

            migrationBuilder.RenameColumn(
                name: "payment_method_id",
                table: "order_payment",
                newName: "payment_method"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "payment_method",
                table: "fund",
                newName: "payment_method_id"
            );
            migrationBuilder.RenameColumn(
                name: "payment_method",
                table: "order_payment",
                newName: "payment_method_id"
            );

            migrationBuilder.CreateTable(
                name: "payment_method",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "citext", nullable: false),
                    created_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_method", x => x.id);
                }
            );

            migrationBuilder.AddColumn<string>(
                name: "payment_method_id",
                table: "order",
                type: "varchar",
                nullable: false,
                defaultValue: (byte)0
            );
            migrationBuilder.AddForeignKey(
                name: "fk_fund_payment_method_payment_method_id",
                table: "fund",
                column: "payment_method_id",
                principalTable: "payment_method",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade
            );
            migrationBuilder.AddForeignKey(
                name: "fk_order_payment_method_payment_method_id",
                table: "order",
                column: "payment_method_id",
                principalTable: "payment_method",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade
            );
            migrationBuilder.AddForeignKey(
                name: "fk_order_payment_payment_method_payment_method_id",
                table: "order_payment",
                column: "payment_method_id",
                principalTable: "payment_method",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade
            );
        }
    }
}
