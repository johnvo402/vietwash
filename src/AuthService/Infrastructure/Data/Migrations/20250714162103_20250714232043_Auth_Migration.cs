using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20250714232043_Auth_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "expired_time", table: "account_token");

            migrationBuilder.AddColumn<int>(
                name: "expired_time",
                table: "account_token",
                type: "integer",
                nullable: false,
                defaultValue: 0
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "expired_time", table: "account_token");
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "expired_time",
                table: "account_token",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()"
            );
        }
    }
}
