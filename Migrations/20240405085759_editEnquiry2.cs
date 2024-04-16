using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GlassECommerce.Migrations
{
    public partial class editEnquiry2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "Enquiries",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Enquiries_ModelId",
                table: "Enquiries",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Enquiries_ProductId",
                table: "Enquiries",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Enquiries_Models_ModelId",
                table: "Enquiries",
                column: "ModelId",
                principalTable: "Models",
                principalColumn: "ModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Enquiries_Products_ProductId",
                table: "Enquiries",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enquiries_Models_ModelId",
                table: "Enquiries");

            migrationBuilder.DropForeignKey(
                name: "FK_Enquiries_Products_ProductId",
                table: "Enquiries");

            migrationBuilder.DropIndex(
                name: "IX_Enquiries_ModelId",
                table: "Enquiries");

            migrationBuilder.DropIndex(
                name: "IX_Enquiries_ProductId",
                table: "Enquiries");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "Enquiries");
        }
    }
}
