using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20250622233317_Finance_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<object>(
                name: "metadata",
                table: "fund",
                type: "jsonb",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_fund_behavior_id",
                table: "fund_behavior",
                column: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_fund_behavior_id",
                table: "fund_behavior");

            migrationBuilder.DropColumn(
                name: "metadata",
                table: "fund");
        }
    }
}
