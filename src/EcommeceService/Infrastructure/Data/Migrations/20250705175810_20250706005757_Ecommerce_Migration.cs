using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20250706005757_Ecommerce_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "product");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "end_at",
                table: "tariff",
                type: "timestamp with time zone",
                nullable: true
            );

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "start_at",
                table: "tariff",
                type: "timestamp with time zone",
                nullable: true
            );

            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "tariff",
                type: "integer",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AlterColumn<decimal>(
                name: "id",
                table: "repair_history",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric"
            );

            migrationBuilder.AddColumn<decimal>(
                name: "capital_price",
                table: "branch_product",
                type: "numeric",
                nullable: false,
                defaultValue: 0m
            );

            migrationBuilder.AddColumn<long>(
                name: "category_id",
                table: "branch_product",
                type: "bigint",
                nullable: false,
                defaultValue: 0L
            );

            migrationBuilder.AddColumn<string>(
                name: "public_id",
                table: "branch_product",
                type: "character varying(26)",
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<long>(
                name: "version",
                table: "branch_product",
                type: "bigint",
                nullable: false,
                defaultValue: 0L
            );

            migrationBuilder.CreateIndex(
                name: "ix_branch_product_category_id",
                table: "branch_product",
                column: "category_id"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_branch_product_category_category_id",
                table: "branch_product",
                column: "category_id",
                principalTable: "category",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_branch_product_category_category_id",
                table: "branch_product"
            );

            migrationBuilder.DropIndex(
                name: "ix_branch_product_category_id",
                table: "branch_product"
            );

            migrationBuilder.DropColumn(name: "end_at", table: "tariff");

            migrationBuilder.DropColumn(name: "start_at", table: "tariff");

            migrationBuilder.DropColumn(name: "status", table: "tariff");

            migrationBuilder.DropColumn(name: "capital_price", table: "branch_product");

            migrationBuilder.DropColumn(name: "category_id", table: "branch_product");

            migrationBuilder.DropColumn(name: "public_id", table: "branch_product");

            migrationBuilder.DropColumn(name: "version", table: "branch_product");

            migrationBuilder.AlterColumn<decimal>(
                name: "id",
                table: "repair_history",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)"
            );

            migrationBuilder.CreateTable(
                name: "product",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    barcode = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    disable = table.Column<bool>(type: "boolean", nullable: false),
                    image = table.Column<string>(type: "text", nullable: true),
                    name = table.Column<string>(type: "text", nullable: false),
                    public_id = table.Column<string>(
                        type: "character varying(26)",
                        nullable: false
                    ),
                    recommended_price = table.Column<decimal>(type: "numeric", nullable: false),
                    sku = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    version = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product", x => x.id);
                }
            );

            migrationBuilder.CreateIndex(name: "ix_product_id", table: "product", column: "id");
        }
    }
}
