using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GlassECommerce.Migrations
{
    public partial class editEquiry : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CollectionId",
                table: "Enquiries",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CollectionId",
                table: "Enquiries");
        }
    }
}
