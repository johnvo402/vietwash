using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20250515163241_Project_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:citext", ",,");

            migrationBuilder.CreateTable(
                name: "category",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "citext", nullable: false),
                    public_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_category", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fund_behavior",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "citext", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fund_behavior", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fund_type",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "citext", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fund_type", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "group",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "citext", nullable: false),
                    description = table.Column<string>(type: "citext", nullable: true),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    disable = table.Column<bool>(type: "boolean", nullable: false),
                    public_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_group", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "inventory_document",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    to_warehouse_id = table.Column<long>(type: "bigint", nullable: true),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    total = table.Column<decimal>(type: "numeric", nullable: false),
                    payment_method = table.Column<int>(type: "integer", nullable: false),
                    paid_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    paid_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    branch_id = table.Column<long>(type: "bigint", nullable: true),
                    from_warehouse_id = table.Column<long>(type: "bigint", nullable: true),
                    transaction_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    code = table.Column<string>(type: "citext", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    note = table.Column<string>(type: "text", nullable: true),
                    public_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inventory_document", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "inventory_invoice",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    supplier_id = table.Column<long>(type: "bigint", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    public_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inventory_invoice", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "inventory_request",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    supplier_id = table.Column<long>(type: "bigint", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    note = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    request_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    branch_id = table.Column<long>(type: "bigint", nullable: true),
                    cancel_reason = table.Column<string>(type: "text", nullable: false),
                    from_warehouse_id = table.Column<long>(type: "bigint", nullable: true),
                    to_warehouse_id = table.Column<long>(type: "bigint", nullable: true),
                    public_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inventory_request", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "product",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    description = table.Column<string>(type: "text", nullable: false),
                    sku = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    barcode = table.Column<string>(type: "text", nullable: false),
                    recommended_price = table.Column<decimal>(type: "numeric", nullable: false),
                    public_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tariff",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "citext", nullable: false),
                    disable = table.Column<bool>(type: "boolean", nullable: false),
                    public_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tariff", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "unit",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "citext", nullable: false),
                    public_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_unit", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    first_name = table.Column<string>(type: "text", nullable: false),
                    last_name = table.Column<string>(type: "text", nullable: false),
                    username = table.Column<string>(type: "citext", nullable: false),
                    email = table.Column<string>(type: "citext", nullable: false),
                    phone_number = table.Column<string>(type: "text", nullable: false),
                    day_of_birth = table.Column<DateTime>(type: "date", nullable: true),
                    gender = table.Column<int>(type: "integer", nullable: true),
                    avatar = table.Column<string>(type: "text", nullable: true),
                    customer_type = table.Column<byte>(type: "smallint", nullable: true),
                    status = table.Column<byte>(type: "smallint", nullable: false),
                    role_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    public_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "service",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "citext", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    image = table.Column<string>(type: "text", nullable: true),
                    disable = table.Column<bool>(type: "boolean", nullable: false),
                    category_id = table.Column<long>(type: "bigint", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    public_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service", x => x.id);
                    table.ForeignKey(
                        name: "fk_service_category_category_id",
                        column: x => x.category_id,
                        principalTable: "category",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "fund",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "citext", nullable: false),
                    type_id = table.Column<string>(type: "text", nullable: false),
                    behavior_id = table.Column<string>(type: "text", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    note = table.Column<string>(type: "text", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    transaction_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    payment_method = table.Column<byte>(type: "smallint", nullable: false),
                    reference_id = table.Column<long>(type: "bigint", nullable: true),
                    public_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fund", x => x.id);
                    table.ForeignKey(
                        name: "fk_fund_fund_behavior_behavior_id",
                        column: x => x.behavior_id,
                        principalTable: "fund_behavior",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_fund_fund_type_type_id",
                        column: x => x.type_id,
                        principalTable: "fund_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "inventory_relation",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    inventory_document_id = table.Column<long>(type: "bigint", nullable: true),
                    inventory_invoice_id = table.Column<long>(type: "bigint", nullable: true),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    public_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inventory_relation", x => x.id);
                    table.ForeignKey(
                        name: "fk_inventory_relation_inventory_document_inventory_document_id",
                        column: x => x.inventory_document_id,
                        principalTable: "inventory_document",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_inventory_relation_inventory_invoice_inventory_invoice_id",
                        column: x => x.inventory_invoice_id,
                        principalTable: "inventory_invoice",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "order",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "text", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    total = table.Column<decimal>(type: "numeric", nullable: false),
                    discount_type = table.Column<bool>(type: "boolean", nullable: false),
                    discount_value = table.Column<decimal>(type: "numeric", nullable: false),
                    customer_id = table.Column<long>(type: "bigint", nullable: true),
                    note = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<byte>(type: "smallint", nullable: false),
                    order_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    received_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    public_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order", x => x.id);
                    table.ForeignKey(
                        name: "fk_order_user_customer_id",
                        column: x => x.customer_id,
                        principalTable: "user",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "unit_relation",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    service_id = table.Column<long>(type: "bigint", nullable: false),
                    unit_id = table.Column<long>(type: "bigint", nullable: false),
                    base_unit = table.Column<bool>(type: "boolean", nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    public_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_unit_relation", x => x.id);
                    table.ForeignKey(
                        name: "fk_unit_relation_service_service_id",
                        column: x => x.service_id,
                        principalTable: "service",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_unit_relation_unit_unit_id",
                        column: x => x.unit_id,
                        principalTable: "unit",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_payment",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_id = table.Column<long>(type: "bigint", nullable: false),
                    payment_method = table.Column<byte>(type: "smallint", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    payment_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    public_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_payment", x => x.id);
                    table.ForeignKey(
                        name: "fk_order_payment_order_order_id",
                        column: x => x.order_id,
                        principalTable: "order",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "group_service",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    service_id = table.Column<long>(type: "bigint", nullable: false),
                    group_id = table.Column<long>(type: "bigint", nullable: false),
                    unit_relation_id = table.Column<long>(type: "bigint", nullable: false),
                    public_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_group_service", x => x.id);
                    table.ForeignKey(
                        name: "fk_group_service_group_group_id",
                        column: x => x.group_id,
                        principalTable: "group",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_group_service_service_service_id",
                        column: x => x.service_id,
                        principalTable: "service",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_group_service_unit_relation_unit_relation_id",
                        column: x => x.unit_relation_id,
                        principalTable: "unit_relation",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_item",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_id = table.Column<long>(type: "bigint", nullable: false),
                    service_id = table.Column<long>(type: "bigint", nullable: false),
                    unit_relation_id = table.Column<long>(type: "bigint", nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    public_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_item", x => x.id);
                    table.ForeignKey(
                        name: "fk_order_item_order_order_id",
                        column: x => x.order_id,
                        principalTable: "order",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_order_item_service_service_id",
                        column: x => x.service_id,
                        principalTable: "service",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_order_item_unit_relation_unit_relation_id",
                        column: x => x.unit_relation_id,
                        principalTable: "unit_relation",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "service_tariff",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tariff_id = table.Column<long>(type: "bigint", nullable: true),
                    service_id = table.Column<long>(type: "bigint", nullable: true),
                    unit_relation_id = table.Column<long>(type: "bigint", nullable: true),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    public_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_tariff", x => x.id);
                    table.ForeignKey(
                        name: "fk_service_tariff_service_service_id",
                        column: x => x.service_id,
                        principalTable: "service",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_service_tariff_tariff_tariff_id",
                        column: x => x.tariff_id,
                        principalTable: "tariff",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_service_tariff_unit_relation_unit_relation_id",
                        column: x => x.unit_relation_id,
                        principalTable: "unit_relation",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_category_id",
                table: "category",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_fund_behavior_id",
                table: "fund",
                column: "behavior_id");

            migrationBuilder.CreateIndex(
                name: "ix_fund_id",
                table: "fund",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_fund_type_id",
                table: "fund",
                column: "type_id");

            migrationBuilder.CreateIndex(
                name: "ix_fund_behavior_id1",
                table: "fund_behavior",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_fund_type_id1",
                table: "fund_type",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_group_id",
                table: "group",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_group_service_group_id",
                table: "group_service",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "ix_group_service_service_id",
                table: "group_service",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "ix_group_service_unit_relation_id",
                table: "group_service",
                column: "unit_relation_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_document_id",
                table: "inventory_document",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_invoice_id",
                table: "inventory_invoice",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_relation_id",
                table: "inventory_relation",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_relation_inventory_document_id",
                table: "inventory_relation",
                column: "inventory_document_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_relation_inventory_invoice_id",
                table: "inventory_relation",
                column: "inventory_invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_request_id",
                table: "inventory_request",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_order_customer_id",
                table: "order",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_id",
                table: "order",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_order_item_id",
                table: "order_item",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_order_item_order_id",
                table: "order_item",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_item_service_id",
                table: "order_item",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_item_unit_relation_id",
                table: "order_item",
                column: "unit_relation_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_payment_id",
                table: "order_payment",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_order_payment_order_id",
                table: "order_payment",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_id",
                table: "product",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_service_category_id",
                table: "service",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_id",
                table: "service",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_service_tariff_service_id",
                table: "service_tariff",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_tariff_tariff_id_service_id_unit_relation_id",
                table: "service_tariff",
                columns: new[] { "tariff_id", "service_id", "unit_relation_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_service_tariff_unit_relation_id",
                table: "service_tariff",
                column: "unit_relation_id");

            migrationBuilder.CreateIndex(
                name: "ix_tariff_id",
                table: "tariff",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_unit_id",
                table: "unit",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_unit_relation_id",
                table: "unit_relation",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_unit_relation_service_id",
                table: "unit_relation",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "ix_unit_relation_unit_id",
                table: "unit_relation",
                column: "unit_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_email",
                table: "user",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_id",
                table: "user",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_user_username",
                table: "user",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fund");

            migrationBuilder.DropTable(
                name: "group_service");

            migrationBuilder.DropTable(
                name: "inventory_relation");

            migrationBuilder.DropTable(
                name: "inventory_request");

            migrationBuilder.DropTable(
                name: "order_item");

            migrationBuilder.DropTable(
                name: "order_payment");

            migrationBuilder.DropTable(
                name: "product");

            migrationBuilder.DropTable(
                name: "service_tariff");

            migrationBuilder.DropTable(
                name: "fund_behavior");

            migrationBuilder.DropTable(
                name: "fund_type");

            migrationBuilder.DropTable(
                name: "group");

            migrationBuilder.DropTable(
                name: "inventory_document");

            migrationBuilder.DropTable(
                name: "inventory_invoice");

            migrationBuilder.DropTable(
                name: "order");

            migrationBuilder.DropTable(
                name: "tariff");

            migrationBuilder.DropTable(
                name: "unit_relation");

            migrationBuilder.DropTable(
                name: "user");

            migrationBuilder.DropTable(
                name: "service");

            migrationBuilder.DropTable(
                name: "unit");

            migrationBuilder.DropTable(
                name: "category");
        }
    }
}
