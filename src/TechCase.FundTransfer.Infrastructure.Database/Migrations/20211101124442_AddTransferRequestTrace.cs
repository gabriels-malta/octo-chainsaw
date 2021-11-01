using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TechCase.FundTransfer.Infrastructure.Database.Migrations
{
    public partial class AddTransferRequestTrace : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TransferRequestId",
                table: "Transactions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TransferRequestId",
                table: "Transactions");
        }
    }
}
