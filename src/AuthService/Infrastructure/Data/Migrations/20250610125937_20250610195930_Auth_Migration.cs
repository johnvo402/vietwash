using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20250610195930_Auth_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "email_enabled", table: "account");

            migrationBuilder.DropColumn(name: "phone_enabled", table: "account");

            migrationBuilder.AlterColumn<string>(
                name: "password",
                table: "account",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text"
            );

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "account",
                type: "citext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "citext"
            );

            migrationBuilder.AddColumn<string>(
                name: "phone_code",
                table: "account",
                type: "text",
                nullable: false,
                defaultValue: "+84"
            );

            migrationBuilder.AddColumn<bool>(
                name: "verified",
                table: "account",
                type: "boolean",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.CreateIndex(
                name: "ix_account_phone_number",
                table: "account",
                column: "phone_number",
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "ix_account_phone_number", table: "account");

            migrationBuilder.DropColumn(name: "phone_code", table: "account");

            migrationBuilder.DropColumn(name: "verified", table: "account");

            migrationBuilder.AlterColumn<string>(
                name: "password",
                table: "account",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "account",
                type: "citext",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "citext",
                oldNullable: true
            );

            migrationBuilder.AddColumn<bool>(
                name: "email_enabled",
                table: "account",
                type: "boolean",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<bool>(
                name: "phone_enabled",
                table: "account",
                type: "boolean",
                nullable: false,
                defaultValue: false
            );
        }
    }
}
