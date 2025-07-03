using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20250704011058_Ecommerce_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "public_id",
                table: "voucher_customer");

            migrationBuilder.DropColumn(
                name: "public_id",
                table: "unit");

            migrationBuilder.DropColumn(
                name: "public_id",
                table: "service_tariff");

            migrationBuilder.DropColumn(
                name: "public_id",
                table: "service_price_tariff_history");

            migrationBuilder.DropColumn(
                name: "arrive_at",
                table: "product_supplying");

            migrationBuilder.DropColumn(
                name: "expery_date",
                table: "product_supplying");

            migrationBuilder.DropColumn(
                name: "public_id",
                table: "product_supplying");

            migrationBuilder.DropColumn(
                name: "type",
                table: "product_supplying");

            migrationBuilder.DropColumn(
                name: "public_id",
                table: "order_payment");

            migrationBuilder.DropColumn(
                name: "public_id",
                table: "order_item");

            migrationBuilder.DropColumn(
                name: "public_id",
                table: "inventory_relation");

            migrationBuilder.DropColumn(
                name: "paid_at",
                table: "inventory_document");

            migrationBuilder.DropColumn(
                name: "total",
                table: "inventory_document");

            migrationBuilder.DropColumn(
                name: "arrived_at",
                table: "equipment_supplying");

            migrationBuilder.DropColumn(
                name: "expiry_date",
                table: "equipment_supplying");

            migrationBuilder.DropColumn(
                name: "public_id",
                table: "equipment_supplying");

            migrationBuilder.DropColumn(
                name: "type",
                table: "equipment_supplying");

            migrationBuilder.DropColumn(
                name: "note",
                table: "equipment");

            migrationBuilder.DropColumn(
                name: "public_id",
                table: "category");

            migrationBuilder.DropColumn(
                name: "public_id",
                table: "branch_user");

            migrationBuilder.AlterColumn<decimal>(
                name: "id",
                table: "repair_history",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "expiry_date",
                table: "product_supplying",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "cancel_reason",
                table: "inventory_request",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<long>(
                name: "version",
                table: "inventory_request",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "transaction_at",
                table: "inventory_invoice",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<long>(
                name: "version",
                table: "inventory_invoice",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<short>(
                name: "status",
                table: "inventory_document",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<byte>(
                name: "payment_method",
                table: "inventory_document",
                type: "smallint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "arrived_at",
                table: "inventory_document",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cancel_reason",
                table: "inventory_document",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "quantity",
                table: "equipment_supplying",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "equipment",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "image",
                table: "equipment",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "expiry_date",
                table: "product_supplying");

            migrationBuilder.DropColumn(
                name: "version",
                table: "inventory_request");

            migrationBuilder.DropColumn(
                name: "transaction_at",
                table: "inventory_invoice");

            migrationBuilder.DropColumn(
                name: "version",
                table: "inventory_invoice");

            migrationBuilder.DropColumn(
                name: "arrived_at",
                table: "inventory_document");

            migrationBuilder.DropColumn(
                name: "cancel_reason",
                table: "inventory_document");

            migrationBuilder.DropColumn(
                name: "quantity",
                table: "equipment_supplying");

            migrationBuilder.DropColumn(
                name: "image",
                table: "equipment");

            migrationBuilder.AddColumn<string>(
                name: "public_id",
                table: "voucher_customer",
                type: "character varying(26)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "public_id",
                table: "unit",
                type: "character varying(26)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "public_id",
                table: "service_tariff",
                type: "character varying(26)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "public_id",
                table: "service_price_tariff_history",
                type: "character varying(26)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<decimal>(
                name: "id",
                table: "repair_history",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "arrive_at",
                table: "product_supplying",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "expery_date",
                table: "product_supplying",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "public_id",
                table: "product_supplying",
                type: "character varying(26)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<short>(
                name: "type",
                table: "product_supplying",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<string>(
                name: "public_id",
                table: "order_payment",
                type: "character varying(26)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "public_id",
                table: "order_item",
                type: "character varying(26)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "cancel_reason",
                table: "inventory_request",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "public_id",
                table: "inventory_relation",
                type: "character varying(26)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "status",
                table: "inventory_document",
                type: "integer",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<int>(
                name: "payment_method",
                table: "inventory_document",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(byte),
                oldType: "smallint",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "paid_at",
                table: "inventory_document",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "total",
                table: "inventory_document",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "arrived_at",
                table: "equipment_supplying",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "expiry_date",
                table: "equipment_supplying",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "public_id",
                table: "equipment_supplying",
                type: "character varying(26)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<short>(
                name: "type",
                table: "equipment_supplying",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "equipment",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "note",
                table: "equipment",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "public_id",
                table: "category",
                type: "character varying(26)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "public_id",
                table: "branch_user",
                type: "character varying(26)",
                nullable: false,
                defaultValue: "");
        }
    }
}
