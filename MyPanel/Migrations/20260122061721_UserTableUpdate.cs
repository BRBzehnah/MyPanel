using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyPanel.Migrations
{
    /// <inheritdoc />
    public partial class UserTableUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "GuardCode",
                table: "Bots",
                newName: "RestoreCode");

            migrationBuilder.AddColumn<string>(
                name: "MaFilePath",
                table: "Bots",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Bots",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaFilePath",
                table: "Bots");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Bots");

            migrationBuilder.RenameColumn(
                name: "RestoreCode",
                table: "Bots",
                newName: "GuardCode");
        }
    }
}
