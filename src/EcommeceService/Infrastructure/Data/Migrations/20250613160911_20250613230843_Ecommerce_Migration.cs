using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20250613230843_Ecommerce_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var basePath = AppContext.BaseDirectory;

            migrationBuilder.DropTable(name: "fund");

            migrationBuilder.DropTable(name: "fund_behavior");

            migrationBuilder.DropTable(name: "fund_type");

            migrationBuilder.AlterColumn<decimal>(
                name: "id",
                table: "repair_history",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric"
            );
            var fnPath = Path.Combine(
                basePath,
                "Data",
                "Migrations",
                "get_order_summary",
                "up.sql"
            );
            var sql = File.ReadAllText(fnPath);
            migrationBuilder.Sql(sql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var basePath = AppContext.BaseDirectory;

            migrationBuilder.AlterColumn<decimal>(
                name: "id",
                table: "repair_history",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)"
            );

            migrationBuilder.CreateTable(
                name: "fund_behavior",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    name = table.Column<string>(type: "citext", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fund_behavior", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "fund_type",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    name = table.Column<string>(type: "citext", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fund_type", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "fund",
                columns: table => new
                {
                    id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    behavior_id = table.Column<string>(type: "text", nullable: false),
                    type_id = table.Column<string>(type: "text", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "citext", nullable: false),
                    note = table.Column<string>(type: "text", nullable: false),
                    order_id = table.Column<long>(type: "bigint", nullable: true),
                    payment_method = table.Column<byte>(type: "smallint", nullable: false),
                    public_id = table.Column<string>(
                        type: "character varying(26)",
                        nullable: false
                    ),
                    reference_id = table.Column<long>(type: "bigint", nullable: true),
                    transaction_date = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    updated_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    version = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fund", x => x.id);
                    table.ForeignKey(
                        name: "fk_fund_fund_behavior_behavior_id",
                        column: x => x.behavior_id,
                        principalTable: "fund_behavior",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "fk_fund_fund_type_type_id",
                        column: x => x.type_id,
                        principalTable: "fund_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "fk_fund_order_order_id",
                        column: x => x.order_id,
                        principalTable: "order",
                        principalColumn: "id"
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "ix_fund_behavior_id",
                table: "fund",
                column: "behavior_id"
            );

            migrationBuilder.CreateIndex(name: "ix_fund_id", table: "fund", column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_fund_order_id",
                table: "fund",
                column: "order_id"
            );

            migrationBuilder.CreateIndex(name: "ix_fund_type_id", table: "fund", column: "type_id");

            migrationBuilder.CreateIndex(
                name: "ix_fund_behavior_id1",
                table: "fund_behavior",
                column: "id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_fund_type_id1",
                table: "fund_type",
                column: "id"
            );
            var fnPath = Path.Combine(
                basePath,
                "Data",
                "Migrations",
                "get_order_summary",
                "down.sql"
            );
            var sql = File.ReadAllText(fnPath);
            migrationBuilder.Sql(sql);
        }
    }
}
