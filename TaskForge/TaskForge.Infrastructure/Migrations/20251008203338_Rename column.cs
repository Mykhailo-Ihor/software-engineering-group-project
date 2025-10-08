using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Renamecolumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "Users",
                newName: "Auth0UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Auth0UserId",
                table: "Users",
                newName: "PasswordHash");
        }
    }
}
