using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20250629234833_Finance_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM fund_behavior;");
            migrationBuilder.DropColumn(name: "name", table: "fund_behavior");
            migrationBuilder.AddColumn<object>(
                name: "name",
                table: "fund_behavior",
                type: "jsonb",
                nullable: false
            );
            migrationBuilder.AddColumn<bool>(
                name: "automatic",
                table: "fund_behavior",
                type: "boolean",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<bool>(
                name: "generate",
                table: "fund_behavior",
                type: "boolean",
                nullable: false,
                defaultValue: false
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM fund_behavior;");
            migrationBuilder.DropColumn(name: "automatic", table: "fund_behavior");

            migrationBuilder.DropColumn(name: "generate", table: "fund_behavior");

            migrationBuilder.DropColumn(name: "name", table: "fund_behavior");
            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "fund_behavior",
                type: "text",
                nullable: false
            );
        }
    }
}
