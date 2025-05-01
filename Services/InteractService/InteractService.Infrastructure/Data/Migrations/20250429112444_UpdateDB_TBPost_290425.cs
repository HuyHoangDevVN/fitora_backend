using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InteractService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDB_TBPost_290425 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Score",
                table: "Posts",
                type: "double",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Score",
                table: "Posts");
        }
    }
}
