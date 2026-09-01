using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFundSourceEventId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "source_event_id",
                table: "fund",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_fund_source_event_id",
                table: "fund",
                column: "source_event_id",
                unique: true,
                filter: "source_event_id IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_fund_source_event_id",
                table: "fund");

            migrationBuilder.DropColumn(
                name: "source_event_id",
                table: "fund");
        }
    }
}
