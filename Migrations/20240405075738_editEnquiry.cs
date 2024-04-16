using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GlassECommerce.Migrations
{
    public partial class editEnquiry : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ModelId",
                table: "Enquiries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProducId",
                table: "Enquiries",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModelId",
                table: "Enquiries");

            migrationBuilder.DropColumn(
                name: "ProducId",
                table: "Enquiries");
        }
    }
}
