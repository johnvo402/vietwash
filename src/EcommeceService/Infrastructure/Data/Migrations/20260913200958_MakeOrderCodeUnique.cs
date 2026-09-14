using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class MakeOrderCodeUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_order_code",
                table: "order");

            migrationBuilder.Sql(
                """
                WITH duplicate_codes AS (
                    SELECT id,
                           code,
                           ROW_NUMBER() OVER (PARTITION BY code ORDER BY id) AS duplicate_number
                    FROM "order"
                )
                UPDATE "order" AS orders
                SET code = duplicate_codes.code
                    || '-duplicate-'
                    || orders.id::text
                    || '-'
                    || substr(md5(duplicate_codes.code || ':' || orders.id::text), 1, 8)
                FROM duplicate_codes
                WHERE orders.id = duplicate_codes.id
                  AND duplicate_codes.duplicate_number > 1;
                """
            );

            migrationBuilder.CreateIndex(
                name: "ix_order_code",
                table: "order",
                column: "code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_order_code",
                table: "order");

            migrationBuilder.CreateIndex(
                name: "ix_order_code",
                table: "order",
                column: "code");
        }
    }
}
