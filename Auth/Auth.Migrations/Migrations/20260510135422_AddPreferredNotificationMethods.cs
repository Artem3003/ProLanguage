using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddPreferredNotificationMethods : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PreferredNotificationMethods",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreferredNotificationMethods",
                table: "AspNetUsers");
        }
    }
}
