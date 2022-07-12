using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesTransactionCore.Migrations
{
    public partial class updateSalesTransactionModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sailing_Customers_CustomerId",
                table: "Sailing");

            migrationBuilder.DropForeignKey(
                name: "FK_Sailing_Products_ProductId",
                table: "Sailing");

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "Sailing",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "Sailing",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Sailing_Customers_CustomerId",
                table: "Sailing",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Sailing_Products_ProductId",
                table: "Sailing",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sailing_Customers_CustomerId",
                table: "Sailing");

            migrationBuilder.DropForeignKey(
                name: "FK_Sailing_Products_ProductId",
                table: "Sailing");

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "Sailing",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "Sailing",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Sailing_Customers_CustomerId",
                table: "Sailing",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sailing_Products_ProductId",
                table: "Sailing",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId");
        }
    }
}
