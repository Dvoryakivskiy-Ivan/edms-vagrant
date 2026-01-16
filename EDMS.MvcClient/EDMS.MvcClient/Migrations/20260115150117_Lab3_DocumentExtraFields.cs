using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDMS.MvcClient.Migrations
{
    /// <inheritdoc />
    public partial class Lab3_DocumentExtraFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "Documents",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Confidentiality",
                table: "Documents",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DueAtUtc",
                table: "Documents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "Documents",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "Documents",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "Documents",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documents_Confidentiality",
                table: "Documents",
                column: "Confidentiality");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_DueAtUtc",
                table: "Documents",
                column: "DueAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_Priority",
                table: "Documents",
                column: "Priority");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Documents_Confidentiality",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_DueAtUtc",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_Priority",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "Amount",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "Confidentiality",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "DueAtUtc",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "Documents");
        }
    }
}
