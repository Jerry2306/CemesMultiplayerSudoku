using Microsoft.EntityFrameworkCore;

namespace CemesMultiplayerSudoku.GameSession.Services.Database;

public class SudokuContext : DbContext
{
    public DbSet<SudokuEntity> Sudokus { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer(@"Data Source=.\SQLEXPRESS;Initial Catalog=CemesSudoku;Integrated Security=true;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SudokuEntity>().HasKey(x => x.Id);
        modelBuilder.Entity<SudokuEntity>().Property(x => x.Id).IsRequired().ValueGeneratedOnAdd();

        modelBuilder.Entity<SudokuEntity>().Property(x => x.Unsolved).HasMaxLength(81).IsRequired();
        modelBuilder.Entity<SudokuEntity>().Property(x => x.Solved).HasMaxLength(81).IsRequired();
        modelBuilder.Entity<SudokuEntity>().Property(x => x.Missing).IsRequired();
    }
}