using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20250622174039_Ecommerce_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "customer_id", table: "order_summary_result");

            migrationBuilder.AlterColumn<decimal>(
                name: "id",
                table: "repair_history",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric"
            );

            migrationBuilder.AddColumn<string>(
                name: "customer_name",
                table: "order_summary_result",
                type: "text",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "public_id",
                table: "order_summary_result",
                type: "character varying(26)",
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.CreateIndex(
                name: "ix_order_staff_id",
                table: "order",
                column: "staff_id"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_order_user_staff_id",
                table: "order",
                column: "staff_id",
                principalTable: "user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "fk_order_user_staff_id", table: "order");

            migrationBuilder.DropIndex(name: "ix_order_staff_id", table: "order");

            migrationBuilder.DropColumn(name: "customer_name", table: "order_summary_result");

            migrationBuilder.DropColumn(name: "public_id", table: "order_summary_result");

            migrationBuilder.AlterColumn<decimal>(
                name: "id",
                table: "repair_history",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)"
            );

            migrationBuilder.AddColumn<long>(
                name: "customer_id",
                table: "order_summary_result",
                type: "bigint",
                nullable: false,
                defaultValue: 0L
            );
        }
    }
}
