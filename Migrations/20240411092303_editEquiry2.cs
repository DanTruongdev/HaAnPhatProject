using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GlassECommerce.Migrations
{
    public partial class editEquiry2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Enquiries_CollectionId",
                table: "Enquiries",
                column: "CollectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Enquiries_Collections_CollectionId",
                table: "Enquiries",
                column: "CollectionId",
                principalTable: "Collections",
                principalColumn: "CollectionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enquiries_Collections_CollectionId",
                table: "Enquiries");

            migrationBuilder.DropIndex(
                name: "IX_Enquiries_CollectionId",
                table: "Enquiries");
        }
    }
}
