using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20250313002415_Project_Migration : Migration
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
                    id = table.Column<string>(type: "character varying(26)", nullable: false),
                    name = table.Column<string>(type: "citext", nullable: false),
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
                    id = table.Column<string>(type: "character varying(26)", nullable: false),
                    name = table.Column<string>(type: "citext", nullable: false),
                    description = table.Column<string>(type: "citext", nullable: true),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    disable = table.Column<bool>(type: "boolean", nullable: false),
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
                name: "payment_method",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "citext", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_method", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tariff",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(26)", nullable: false),
                    name = table.Column<string>(type: "citext", nullable: false),
                    disable = table.Column<bool>(type: "boolean", nullable: false),
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
                    id = table.Column<string>(type: "character varying(26)", nullable: false),
                    name = table.Column<string>(type: "citext", nullable: false),
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
                    id = table.Column<string>(type: "character varying(26)", nullable: false),
                    first_name = table.Column<string>(type: "text", nullable: false),
                    last_name = table.Column<string>(type: "text", nullable: false),
                    username = table.Column<string>(type: "citext", nullable: false),
                    email = table.Column<string>(type: "citext", nullable: false),
                    phone_number = table.Column<string>(type: "text", nullable: false),
                    day_of_birth = table.Column<DateTime>(type: "date", nullable: true),
                    gender = table.Column<int>(type: "integer", nullable: true),
                    avatar = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<byte>(type: "smallint", nullable: false),
                    role_id = table.Column<string>(type: "character varying(26)", nullable: false),
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
                    id = table.Column<string>(type: "character varying(26)", nullable: false),
                    name = table.Column<string>(type: "citext", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    image = table.Column<string>(type: "text", nullable: true),
                    disable = table.Column<bool>(type: "boolean", nullable: false),
                    category_id = table.Column<string>(type: "character varying(26)", nullable: false),
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
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "fund",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(26)", nullable: false),
                    name = table.Column<string>(type: "citext", nullable: false),
                    type_id = table.Column<string>(type: "text", nullable: false),
                    behavior_id = table.Column<string>(type: "text", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    note = table.Column<string>(type: "text", nullable: false),
                    transaction_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    payment_method_id = table.Column<string>(type: "text", nullable: false),
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
                    table.ForeignKey(
                        name: "fk_fund_payment_method_payment_method_id",
                        column: x => x.payment_method_id,
                        principalTable: "payment_method",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(26)", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    total = table.Column<decimal>(type: "numeric", nullable: false),
                    discount_type = table.Column<bool>(type: "boolean", nullable: false),
                    discount_value = table.Column<decimal>(type: "numeric", nullable: false),
                    customer_id = table.Column<string>(type: "character varying(26)", nullable: true),
                    note = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    order_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    payment_method_id = table.Column<string>(type: "text", nullable: false),
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
                        name: "fk_order_payment_method_payment_method_id",
                        column: x => x.payment_method_id,
                        principalTable: "payment_method",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
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
                    id = table.Column<string>(type: "character varying(26)", nullable: false),
                    service_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    unit_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    base_unit = table.Column<bool>(type: "boolean", nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
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
                    id = table.Column<string>(type: "character varying(26)", nullable: false),
                    order_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    payment_method_id = table.Column<string>(type: "text", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    payment_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
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
                    table.ForeignKey(
                        name: "fk_order_payment_payment_method_payment_method_id",
                        column: x => x.payment_method_id,
                        principalTable: "payment_method",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "group_service",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(26)", nullable: false),
                    service_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    group_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    unit_relation_id = table.Column<string>(type: "character varying(26)", nullable: false),
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
                    id = table.Column<string>(type: "character varying(26)", nullable: false),
                    order_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    service_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    unit_relation_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
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
                    id = table.Column<string>(type: "character varying(26)", nullable: false),
                    tariff_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    service_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    unit_relation_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_tariff", x => x.id);
                    table.ForeignKey(
                        name: "fk_service_tariff_service_service_id",
                        column: x => x.service_id,
                        principalTable: "service",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_service_tariff_tariff_tariff_id",
                        column: x => x.tariff_id,
                        principalTable: "tariff",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_service_tariff_unit_relation_unit_relation_id",
                        column: x => x.unit_relation_id,
                        principalTable: "unit_relation",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "ix_fund_payment_method_id",
                table: "fund",
                column: "payment_method_id");

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
                name: "ix_order_customer_id",
                table: "order",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_id",
                table: "order",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_order_payment_method_id",
                table: "order",
                column: "payment_method_id");

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
                name: "ix_order_payment_payment_method_id",
                table: "order_payment",
                column: "payment_method_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_method_id",
                table: "payment_method",
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
                name: "order_item");

            migrationBuilder.DropTable(
                name: "order_payment");

            migrationBuilder.DropTable(
                name: "service_tariff");

            migrationBuilder.DropTable(
                name: "fund_behavior");

            migrationBuilder.DropTable(
                name: "fund_type");

            migrationBuilder.DropTable(
                name: "group");

            migrationBuilder.DropTable(
                name: "order");

            migrationBuilder.DropTable(
                name: "tariff");

            migrationBuilder.DropTable(
                name: "unit_relation");

            migrationBuilder.DropTable(
                name: "payment_method");

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
