using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravisPWalker.Migrations
{
    public partial class ImageMimeTypeAdd : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MimeType",
                table: "ImageStore",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MimeType",
                table: "ImageStore");
        }
    }
}
