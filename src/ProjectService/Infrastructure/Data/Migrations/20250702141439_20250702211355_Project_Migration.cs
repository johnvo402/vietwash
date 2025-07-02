using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20250702211355_Project_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "branch_product");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "branch_product",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    branch_id = table.Column<long>(type: "bigint", nullable: false),
                    barcode = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    description = table.Column<string>(type: "text", nullable: true),
                    img_url = table.Column<string>(type: "text", nullable: true),
                    public_id = table.Column<string>(
                        type: "character varying(26)",
                        nullable: false
                    ),
                    sku = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_branch_product", x => x.id);
                    table.ForeignKey(
                        name: "fk_branch_product_branch_branch_id",
                        column: x => x.branch_id,
                        principalTable: "branch",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "ix_branch_product_branch_id",
                table: "branch_product",
                column: "branch_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_branch_product_id",
                table: "branch_product",
                column: "id"
            );
        }
    }
}
