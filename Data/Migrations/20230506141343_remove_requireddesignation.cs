using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TSAIdentity.Data.Migrations
{
    /// <inheritdoc />
    public partial class remove_requireddesignation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Designations_RequiredDesignationId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_RequiredDesignationId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "RequiredDesignationId",
                table: "Tasks");

            migrationBuilder.AlterColumn<int>(
                name: "TaskStatus",
                table: "Tasks",
                type: "int",
                maxLength: 50,
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TaskStatus",
                table: "Tasks",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<Guid>(
                name: "RequiredDesignationId",
                table: "Tasks",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_RequiredDesignationId",
                table: "Tasks",
                column: "RequiredDesignationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Designations_RequiredDesignationId",
                table: "Tasks",
                column: "RequiredDesignationId",
                principalTable: "Designations",
                principalColumn: "DesignationId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
