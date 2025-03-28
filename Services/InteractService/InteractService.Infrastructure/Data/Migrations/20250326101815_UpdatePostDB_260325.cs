using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InteractService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePostDB_260325 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserVoteds_Posts_PostId1",
                table: "UserVoteds");

            migrationBuilder.DropIndex(
                name: "IX_UserVoteds_PostId1",
                table: "UserVoteds");

            migrationBuilder.DropColumn(
                name: "PostId1",
                table: "UserVoteds");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "UserVoteds",
                type: "char(36)",
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<Guid>(
                name: "PostId",
                table: "UserVoteds",
                type: "char(36)",
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_UserVoteds_PostId",
                table: "UserVoteds",
                column: "PostId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserVoteds_Posts_PostId",
                table: "UserVoteds",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserVoteds_Posts_PostId",
                table: "UserVoteds");

            migrationBuilder.DropIndex(
                name: "IX_UserVoteds_PostId",
                table: "UserVoteds");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "UserVoteds",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AlterColumn<int>(
                name: "PostId",
                table: "UserVoteds",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "PostId1",
                table: "UserVoteds",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_UserVoteds_PostId1",
                table: "UserVoteds",
                column: "PostId1");

            migrationBuilder.AddForeignKey(
                name: "FK_UserVoteds_Posts_PostId1",
                table: "UserVoteds",
                column: "PostId1",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
