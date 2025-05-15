using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20250515154955_Project_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_order_user_customer_id",
                table: "order");

            migrationBuilder.DropForeignKey(
                name: "fk_unit_relation_service_service_id",
                table: "unit_relation");

            migrationBuilder.DropForeignKey(
                name: "fk_unit_relation_unit_unit_id",
                table: "unit_relation");

            migrationBuilder.DropIndex(
                name: "ix_unit_relation_service_id",
                table: "unit_relation");

            migrationBuilder.DropIndex(
                name: "ix_unit_relation_unit_id",
                table: "unit_relation");

            migrationBuilder.DropColumn(
                name: "service_id",
                table: "unit_relation");

            migrationBuilder.DropColumn(
                name: "received_time",
                table: "order");

            migrationBuilder.RenameColumn(
                name: "unit_id",
                table: "unit_relation",
                newName: "public_id");

            migrationBuilder.RenameColumn(
                name: "discount_type",
                table: "order",
                newName: "discount_fixed");

            migrationBuilder.AlterColumn<long>(
                name: "id",
                table: "user",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(26)")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "public_id",
                table: "user",
                type: "character varying(26)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<long>(
                name: "id",
                table: "unit_relation",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(26)")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<long>(
                name: "branch_id",
                table: "unit_relation",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "multiple",
                table: "unit_relation",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "unit_relation",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "processing_time",
                table: "unit_relation",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<long>(
                name: "reference_id",
                table: "unit_relation",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<long>(
                name: "id",
                table: "unit",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(26)")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "path",
                table: "unit",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "public_id",
                table: "unit",
                type: "character varying(26)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<long>(
                name: "id",
                table: "tariff",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(26)")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "public_id",
                table: "tariff",
                type: "character varying(26)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<long>(
                name: "unit_relation_id",
                table: "service_tariff",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(26)");

            migrationBuilder.AlterColumn<long>(
                name: "tariff_id",
                table: "service_tariff",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(26)");

            migrationBuilder.AlterColumn<long>(
                name: "service_id",
                table: "service_tariff",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(26)");

            migrationBuilder.AlterColumn<long>(
                name: "id",
                table: "service_tariff",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(26)")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "public_id",
                table: "service_tariff",
                type: "character varying(26)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "service_id1",
                table: "service_tariff",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "category_id",
                table: "service",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(26)");

            migrationBuilder.AlterColumn<long>(
                name: "id",
                table: "service",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(26)")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<long>(
                name: "branch_id",
                table: "service",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "public_id",
                table: "service",
                type: "character varying(26)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "slug",
                table: "service",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "type",
                table: "service",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AlterColumn<long>(
                name: "order_id",
                table: "order_payment",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(26)");

            migrationBuilder.AlterColumn<long>(
                name: "id",
                table: "order_payment",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(26)")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "public_id",
                table: "order_payment",
                type: "character varying(26)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<long>(
                name: "unit_relation_id",
                table: "order_item",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(26)");

            migrationBuilder.AlterColumn<long>(
                name: "service_id",
                table: "order_item",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(26)");

            migrationBuilder.AlterColumn<long>(
                name: "order_id",
                table: "order_item",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(26)");

            migrationBuilder.AlterColumn<long>(
                name: "id",
                table: "order_item",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(26)")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "public_id",
                table: "order_item",
                type: "character varying(26)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<long>(
                name: "customer_id",
                table: "order",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(string),
                oldType: "character varying(26)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "code",
                table: "order",
                type: "citext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<long>(
                name: "id",
                table: "order",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(26)")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<long>(
                name: "branch_id",
                table: "order",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "delivery_time",
                table: "order",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "public_id",
                table: "order",
                type: "character varying(26)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "staff_id",
                table: "order",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<long>(
                name: "unit_relation_id",
                table: "group_service",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(26)");

            migrationBuilder.AlterColumn<long>(
                name: "service_id",
                table: "group_service",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(26)");

            migrationBuilder.AlterColumn<long>(
                name: "group_id",
                table: "group_service",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(26)");

            migrationBuilder.AlterColumn<long>(
                name: "id",
                table: "group_service",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(26)")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "public_id",
                table: "group_service",
                type: "character varying(26)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<long>(
                name: "id",
                table: "group",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(26)")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "public_id",
                table: "group",
                type: "character varying(26)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<long>(
                name: "reference_id",
                table: "fund",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(26)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "id",
                table: "fund",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(26)")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<long>(
                name: "order_id",
                table: "fund",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "public_id",
                table: "fund",
                type: "character varying(26)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<long>(
                name: "id",
                table: "category",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(26)")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "public_id",
                table: "category",
                type: "character varying(26)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "print_template",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    html_template = table.Column<string>(type: "text", nullable: false),
                    public_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_print_template", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "service_price_tariff_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    service_id = table.Column<long>(type: "bigint", nullable: false),
                    tariff_id = table.Column<long>(type: "bigint", nullable: false),
                    unit_relation_id = table.Column<long>(type: "bigint", nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    public_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_price_tariff_history", x => x.id);
                    table.ForeignKey(
                        name: "fk_service_price_tariff_history_service_service_id",
                        column: x => x.service_id,
                        principalTable: "service",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_service_price_tariff_history_tariff_tariff_id",
                        column: x => x.tariff_id,
                        principalTable: "tariff",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_unit_relation_reference_id",
                table: "unit_relation",
                column: "reference_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_tariff_service_id1",
                table: "service_tariff",
                column: "service_id1");

            migrationBuilder.CreateIndex(
                name: "ix_fund_order_id",
                table: "fund",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_print_template_id",
                table: "print_template",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_service_price_tariff_history_service_id",
                table: "service_price_tariff_history",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_price_tariff_history_tariff_id",
                table: "service_price_tariff_history",
                column: "tariff_id");

            migrationBuilder.AddForeignKey(
                name: "fk_fund_order_order_id",
                table: "fund",
                column: "order_id",
                principalTable: "order",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_order_user_customer_id",
                table: "order",
                column: "customer_id",
                principalTable: "user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_service_tariff_service_service_id1",
                table: "service_tariff",
                column: "service_id1",
                principalTable: "service",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_unit_relation_service_reference_id",
                table: "unit_relation",
                column: "reference_id",
                principalTable: "service",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_fund_order_order_id",
                table: "fund");

            migrationBuilder.DropForeignKey(
                name: "fk_order_user_customer_id",
                table: "order");

            migrationBuilder.DropForeignKey(
                name: "fk_service_tariff_service_service_id1",
                table: "service_tariff");

            migrationBuilder.DropForeignKey(
                name: "fk_unit_relation_service_reference_id",
                table: "unit_relation");

            migrationBuilder.DropTable(
                name: "print_template");

            migrationBuilder.DropTable(
                name: "service_price_tariff_history");

            migrationBuilder.DropIndex(
                name: "ix_unit_relation_reference_id",
                table: "unit_relation");

            migrationBuilder.DropIndex(
                name: "ix_service_tariff_service_id1",
                table: "service_tariff");

            migrationBuilder.DropIndex(
                name: "ix_fund_order_id",
                table: "fund");

            migrationBuilder.DropColumn(
                name: "public_id",
                table: "user");

            migrationBuilder.DropColumn(
                name: "branch_id",
                table: "unit_relation");

            migrationBuilder.DropColumn(
                name: "multiple",
                table: "unit_relation");

            migrationBuilder.DropColumn(
                name: "name",
                table: "unit_relation");

            migrationBuilder.DropColumn(
                name: "processing_time",
                table: "unit_relation");

            migrationBuilder.DropColumn(
                name: "reference_id",
                table: "unit_relation");

            migrationBuilder.DropColumn(
                name: "path",
                table: "unit");

            migrationBuilder.DropColumn(
                name: "public_id",
                table: "unit");

            migrationBuilder.DropColumn(
                name: "public_id",
                table: "tariff");

            migrationBuilder.DropColumn(
                name: "public_id",
                table: "service_tariff");

            migrationBuilder.DropColumn(
                name: "service_id1",
                table: "service_tariff");

            migrationBuilder.DropColumn(
                name: "branch_id",
                table: "service");

            migrationBuilder.DropColumn(
                name: "public_id",
                table: "service");

            migrationBuilder.DropColumn(
                name: "slug",
                table: "service");

            migrationBuilder.DropColumn(
                name: "type",
                table: "service");

            migrationBuilder.DropColumn(
                name: "public_id",
                table: "order_payment");

            migrationBuilder.DropColumn(
                name: "public_id",
                table: "order_item");

            migrationBuilder.DropColumn(
                name: "branch_id",
                table: "order");

            migrationBuilder.DropColumn(
                name: "delivery_time",
                table: "order");

            migrationBuilder.DropColumn(
                name: "public_id",
                table: "order");

            migrationBuilder.DropColumn(
                name: "staff_id",
                table: "order");

            migrationBuilder.DropColumn(
                name: "public_id",
                table: "group_service");

            migrationBuilder.DropColumn(
                name: "public_id",
                table: "group");

            migrationBuilder.DropColumn(
                name: "order_id",
                table: "fund");

            migrationBuilder.DropColumn(
                name: "public_id",
                table: "fund");

            migrationBuilder.DropColumn(
                name: "public_id",
                table: "category");

            migrationBuilder.RenameColumn(
                name: "public_id",
                table: "unit_relation",
                newName: "unit_id");

            migrationBuilder.RenameColumn(
                name: "discount_fixed",
                table: "order",
                newName: "discount_type");

            migrationBuilder.AlterColumn<string>(
                name: "id",
                table: "user",
                type: "character varying(26)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "id",
                table: "unit_relation",
                type: "character varying(26)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "service_id",
                table: "unit_relation",
                type: "character varying(26)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "id",
                table: "unit",
                type: "character varying(26)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "id",
                table: "tariff",
                type: "character varying(26)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "unit_relation_id",
                table: "service_tariff",
                type: "character varying(26)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "tariff_id",
                table: "service_tariff",
                type: "character varying(26)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "service_id",
                table: "service_tariff",
                type: "character varying(26)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "id",
                table: "service_tariff",
                type: "character varying(26)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "category_id",
                table: "service",
                type: "character varying(26)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "id",
                table: "service",
                type: "character varying(26)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "order_id",
                table: "order_payment",
                type: "character varying(26)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "id",
                table: "order_payment",
                type: "character varying(26)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "unit_relation_id",
                table: "order_item",
                type: "character varying(26)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "service_id",
                table: "order_item",
                type: "character varying(26)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "order_id",
                table: "order_item",
                type: "character varying(26)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "id",
                table: "order_item",
                type: "character varying(26)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "customer_id",
                table: "order",
                type: "character varying(26)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "code",
                table: "order",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "citext");

            migrationBuilder.AlterColumn<string>(
                name: "id",
                table: "order",
                type: "character varying(26)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "received_time",
                table: "order",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "unit_relation_id",
                table: "group_service",
                type: "character varying(26)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "service_id",
                table: "group_service",
                type: "character varying(26)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "group_id",
                table: "group_service",
                type: "character varying(26)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "id",
                table: "group_service",
                type: "character varying(26)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "id",
                table: "group",
                type: "character varying(26)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "reference_id",
                table: "fund",
                type: "character varying(26)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "id",
                table: "fund",
                type: "character varying(26)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "id",
                table: "category",
                type: "character varying(26)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.CreateIndex(
                name: "ix_unit_relation_service_id",
                table: "unit_relation",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "ix_unit_relation_unit_id",
                table: "unit_relation",
                column: "unit_id");

            migrationBuilder.AddForeignKey(
                name: "fk_order_user_customer_id",
                table: "order",
                column: "customer_id",
                principalTable: "user",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_unit_relation_service_service_id",
                table: "unit_relation",
                column: "service_id",
                principalTable: "service",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_unit_relation_unit_unit_id",
                table: "unit_relation",
                column: "unit_id",
                principalTable: "unit",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
