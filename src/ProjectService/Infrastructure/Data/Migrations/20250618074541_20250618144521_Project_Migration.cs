using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20250618144521_Project_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(name: "pk_queue_log", table: "queue_log");

            migrationBuilder.RenameTable(name: "queue_log", newName: "pub_sub_log");

            migrationBuilder.AddPrimaryKey(
                name: "pk_pub_sub_log",
                table: "pub_sub_log",
                column: "id"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(name: "pk_pub_sub_log", table: "pub_sub_log");

            migrationBuilder.RenameTable(name: "pub_sub_log", newName: "queue_log");

            migrationBuilder.AddPrimaryKey(name: "pk_queue_log", table: "queue_log", column: "id");
        }
    }
}
