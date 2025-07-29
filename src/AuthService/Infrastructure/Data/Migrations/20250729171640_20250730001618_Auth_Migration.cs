using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20250730001618_Auth_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_account_contact_account_id",
                table: "account_contact");

            migrationBuilder.AlterColumn<byte>(
                name: "status",
                table: "account",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "smallint",
                oldDefaultValue: (byte)1);

            migrationBuilder.CreateIndex(
                name: "ix_account_contact_account_id",
                table: "account_contact",
                column: "account_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_account_contact_account_id",
                table: "account_contact");

            migrationBuilder.AlterColumn<byte>(
                name: "status",
                table: "account",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)1,
                oldClrType: typeof(byte),
                oldType: "smallint");

            migrationBuilder.CreateIndex(
                name: "ix_account_contact_account_id",
                table: "account_contact",
                column: "account_id");
        }
    }
}
