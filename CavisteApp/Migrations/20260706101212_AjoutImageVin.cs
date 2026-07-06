using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CavisteApp.Migrations
{
    /// <inheritdoc />
    public partial class AjoutImageVin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Vins",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Vins");
        }
    }
}
