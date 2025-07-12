using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20250713001734_Finance_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "version", table: "transaction");

            migrationBuilder.AddColumn<object>(
                name: "metadata",
                table: "transaction",
                type: "jsonb",
                nullable: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_transaction_customer_id",
                table: "transaction",
                column: "customer_id"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_transaction_user_customer_id",
                table: "transaction",
                column: "customer_id",
                principalTable: "user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_transaction_user_customer_id",
                table: "transaction"
            );

            migrationBuilder.DropIndex(name: "ix_transaction_customer_id", table: "transaction");

            migrationBuilder.DropColumn(name: "metadata", table: "transaction");

            migrationBuilder.AddColumn<long>(
                name: "version",
                table: "transaction",
                type: "bigint",
                nullable: false,
                defaultValue: 0L
            );
        }
    }
}
