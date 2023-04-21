using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BitRex.Infrastructure.Migrations
{
    public partial class includeHash : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Hash",
                table: "Transactions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Hash",
                table: "Transactions");
        }
    }
}
