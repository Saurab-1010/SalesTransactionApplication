using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesTransactionCore.Migrations
{
    public partial class updateSalesTransactionModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InvoiceId",
                table: "Sailing",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sailing_InvoiceId",
                table: "Sailing",
                column: "InvoiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sailing_Invoices_InvoiceId",
                table: "Sailing",
                column: "InvoiceId",
                principalTable: "Invoices",
                principalColumn: "InvoiceId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sailing_Invoices_InvoiceId",
                table: "Sailing");

            migrationBuilder.DropIndex(
                name: "IX_Sailing_InvoiceId",
                table: "Sailing");

            migrationBuilder.DropColumn(
                name: "InvoiceId",
                table: "Sailing");
        }
    }
}
