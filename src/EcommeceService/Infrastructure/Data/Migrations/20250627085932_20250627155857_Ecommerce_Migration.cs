using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20250627155857_Ecommerce_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "fk_order_user_customer_id", table: "order");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "birth_day",
                table: "user",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date"
            );

            migrationBuilder
                .AlterColumn<long>(
                    name: "id",
                    table: "user",
                    type: "bigint",
                    nullable: false,
                    oldClrType: typeof(long),
                    oldType: "bigint"
                )
                .OldAnnotation(
                    "Npgsql:ValueGenerationStrategy",
                    NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                );

            migrationBuilder.AlterColumn<decimal>(
                name: "id",
                table: "repair_history",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric"
            );

            migrationBuilder.AddColumn<bool>(
                name: "disable",
                table: "product",
                type: "boolean",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<string>(
                name: "image",
                table: "product",
                type: "text",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "product",
                type: "text",
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AlterColumn<long>(
                name: "customer_id",
                table: "order",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_order_user_customer_id",
                table: "order",
                column: "customer_id",
                principalTable: "user",
                principalColumn: "id"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "fk_order_user_customer_id", table: "order");

            migrationBuilder.DropColumn(name: "disable", table: "product");

            migrationBuilder.DropColumn(name: "image", table: "product");

            migrationBuilder.DropColumn(name: "name", table: "product");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "birth_day",
                table: "user",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1),
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true
            );

            migrationBuilder
                .AlterColumn<long>(
                    name: "id",
                    table: "user",
                    type: "bigint",
                    nullable: false,
                    oldClrType: typeof(long),
                    oldType: "bigint"
                )
                .Annotation(
                    "Npgsql:ValueGenerationStrategy",
                    NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                );

            migrationBuilder.AlterColumn<decimal>(
                name: "id",
                table: "repair_history",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)"
            );

            migrationBuilder.AlterColumn<long>(
                name: "customer_id",
                table: "order",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true
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
    }
}
