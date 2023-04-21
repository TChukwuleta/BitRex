using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BitRex.Infrastructure.Migrations
{
    public partial class initialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DestinationAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DestinationPaymentModeType = table.Column<int>(type: "int", nullable: false),
                    SourceAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SourcePaymentModeType = table.Column<int>(type: "int", nullable: true),
                    Narration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DestinationAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SourceAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TransactionStatus = table.Column<int>(type: "int", nullable: false),
                    TransactionReference = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Transactions");
        }
    }
}
