using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyPanel.Migrations
{
    /// <inheritdoc />
    public partial class Bot_model_changes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "Bots");

            migrationBuilder.AddColumn<string>(
                name: "EmailPassword",
                table: "Bots",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailPassword",
                table: "Bots");

            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "Bots",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
