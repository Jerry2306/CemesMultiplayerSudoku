using Microsoft.EntityFrameworkCore.Design;

namespace CemesMultiplayerSudoku.GameSession.Services.Database;

public class SudokuDesignContextFactory : IDesignTimeDbContextFactory<SudokuContext>
{
    // dotnet ef migrations add Initial --output-dir "GameSession/Services/Database/Migrations"

    public SudokuContext CreateDbContext(string[] args)
        => new();
}