using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InteractService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDBVoteComment_080425 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Votes",
                table: "Comments",
                newName: "VotesCont");

            migrationBuilder.AddColumn<string>(
                name: "MediaUrl",
                table: "Comments",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CommentVotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CommentId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    VoteType = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    LastModified = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentVotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommentVotes_Comments_CommentId",
                        column: x => x.CommentId,
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CommentVotes_CommentId",
                table: "CommentVotes",
                column: "CommentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommentVotes");

            migrationBuilder.DropColumn(
                name: "MediaUrl",
                table: "Comments");

            migrationBuilder.RenameColumn(
                name: "VotesCont",
                table: "Comments",
                newName: "Votes");
        }
    }
}
