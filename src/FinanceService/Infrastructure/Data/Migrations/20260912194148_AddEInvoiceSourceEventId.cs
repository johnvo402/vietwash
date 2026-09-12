using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEInvoiceSourceEventId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "source_event_id",
                table: "e_invoice",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_e_invoice_source_event_id",
                table: "e_invoice",
                column: "source_event_id",
                unique: true,
                filter: "source_event_id IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_e_invoice_source_event_id",
                table: "e_invoice");

            migrationBuilder.DropColumn(
                name: "source_event_id",
                table: "e_invoice");
        }
    }
}
