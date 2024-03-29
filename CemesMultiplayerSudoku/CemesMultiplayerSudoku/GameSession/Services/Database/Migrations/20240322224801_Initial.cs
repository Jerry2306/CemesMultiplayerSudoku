using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CemesMultiplayerSudoku.GameSession.Services.Database.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Sudokus",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Unsolved = table.Column<string>(type: "nvarchar(81)", maxLength: 81, nullable: false),
                    Solved = table.Column<string>(type: "nvarchar(81)", maxLength: 81, nullable: false),
                    Missing = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sudokus", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Sudokus");
        }
    }
}
