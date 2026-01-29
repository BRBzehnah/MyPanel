using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyPanel.Migrations
{
    /// <inheritdoc />
    public partial class fieldNameUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MaFilePath",
                table: "Bots",
                newName: "MaFile");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MaFile",
                table: "Bots",
                newName: "MaFilePath");
        }
    }
}
