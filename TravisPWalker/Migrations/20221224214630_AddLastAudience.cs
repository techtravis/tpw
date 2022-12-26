using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravisPWalker.Migrations
{
    public partial class AddLastAudience : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LastAudience",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastAudience",
                table: "AspNetUsers");
        }
    }
}
