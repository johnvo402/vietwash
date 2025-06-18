using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20250618144151_Finance_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "ix_user_username", table: "user");

            migrationBuilder.DropColumn(name: "day_of_birth", table: "user");

            migrationBuilder.DropColumn(name: "role_id", table: "user");

            migrationBuilder.DropColumn(name: "username", table: "user");

            migrationBuilder.RenameColumn(name: "last_name", table: "user", newName: "role");

            migrationBuilder.RenameColumn(
                name: "first_name",
                table: "user",
                newName: "display_name"
            );

            migrationBuilder.RenameColumn(
                name: "customer_type",
                table: "user",
                newName: "customer_group"
            );

            migrationBuilder.RenameColumn(name: "avatar", table: "user", newName: "avt_url");

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "user",
                type: "citext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "citext"
            );

            migrationBuilder.AddColumn<DateOnly>(
                name: "birth_day",
                table: "user",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1)
            );

            migrationBuilder.AddColumn<string>(
                name: "code",
                table: "user",
                type: "text",
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<bool>(
                name: "disabled",
                table: "user",
                type: "boolean",
                nullable: false,
                defaultValue: false
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "birth_day", table: "user");

            migrationBuilder.DropColumn(name: "code", table: "user");

            migrationBuilder.DropColumn(name: "disabled", table: "user");

            migrationBuilder.RenameColumn(name: "role", table: "user", newName: "last_name");

            migrationBuilder.RenameColumn(
                name: "display_name",
                table: "user",
                newName: "first_name"
            );

            migrationBuilder.RenameColumn(
                name: "customer_group",
                table: "user",
                newName: "customer_type"
            );

            migrationBuilder.RenameColumn(name: "avt_url", table: "user", newName: "avatar");

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "user",
                type: "citext",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "citext",
                oldNullable: true
            );

            migrationBuilder.AddColumn<DateTime>(
                name: "day_of_birth",
                table: "user",
                type: "date",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "role_id",
                table: "user",
                type: "character varying(26)",
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<string>(
                name: "username",
                table: "user",
                type: "citext",
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.CreateIndex(
                name: "ix_user_username",
                table: "user",
                column: "username",
                unique: true
            );
        }
    }
}
