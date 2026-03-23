using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaaS.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class UpdateTodoItemSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "TodoItems");

            migrationBuilder.AddColumn<string>(
                name: "AssignedToUserId",
                table: "TodoItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "TodoItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignedToUserId",
                table: "TodoItems");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "TodoItems");

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "TodoItems",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
