using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace CemesMultiplayerSudoku.GameSession.Services.Database;

public class SudokuEntity
{
    public long Id { get; set; }
    public string Unsolved { get; set; } = string.Empty;
    public string Solved { get; set; } = string.Empty;
    public int Missing { get; set; }
}